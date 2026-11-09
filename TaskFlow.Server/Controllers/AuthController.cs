using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TaskFlow.Server.Data;
using TaskFlow.Server.Models;
using TaskFlow.Shared.DTOs;

namespace TaskFlow.Server.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AuthController : ControllerBase
    {
        private readonly AppDbContext _context;

        public AuthController(AppDbContext context)
        {
            _context = context;
        }


        [HttpPost("register")]
        public async Task<ActionResult<UserDto>> Register([FromBody] RegisterDto dto)
        {
            if (string.IsNullOrWhiteSpace(dto.Username) || string.IsNullOrWhiteSpace(dto.Password))
            {
                return BadRequest("Ім'я користувача та пароль є обов'язковими.");
            }

            var existingUser = await _context.Users
                .FirstOrDefaultAsync(u => u.Username.ToLower() == dto.Username.ToLower());

            if (existingUser != null)
            {
                return BadRequest("Користувач з таким ім'ям вже існує.");
            }

            string passwordHash = BCrypt.Net.BCrypt.HashPassword(dto.Password);

            var user = new User
            {
                Username = dto.Username,
                PasswordHash = passwordHash
            };

            _context.Users.Add(user);
            await _context.SaveChangesAsync();

            return Ok(new UserDto
            {
                Id = user.Id,
                Username = user.Username
            });
        }


        [HttpPost("login")]
        public async Task<ActionResult<UserDto>> Login([FromBody] LoginDto dto)
        {

            var user = await _context.Users
                .FirstOrDefaultAsync(u => u.Username.ToLower() == dto.Username.ToLower());

            if (user == null)
            {
                return Unauthorized("Невірне ім'я користувача або пароль.");
            }

            bool isPasswordValid = BCrypt.Net.BCrypt.Verify(dto.Password, user.PasswordHash);

            if (!isPasswordValid)
            {
                return Unauthorized("Невірне ім'я користувача або пароль.");
            }

            return Ok(new UserDto
            {
                Id = user.Id,
                Username = user.Username
            });
        }
    }
}