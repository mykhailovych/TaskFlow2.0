using Microsoft.AspNetCore.Mvc;
using TaskFlow.Server.Data;
using TaskFlow.Server.Models;
using TaskFlow.Shared.DTOs;


namespace TaskFlow.Server.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ColumnsController : ControllerBase
    {
        private readonly AppDbContext _context;

        public ColumnsController(AppDbContext context)
        {
            _context = context;
        }

        [HttpPost]
        public async Task<ActionResult<ColumnDto>> CreateColumn([FromBody] CreateColumnDto dto)
        {
            if(dto == null || string.IsNullOrWhiteSpace(dto.Name))
            {
                return BadRequest("Невірні дані колонки.");
            }
            var column = new Column
            {
                Name = dto.Name,
                ProjectId = dto.ProjectId,
                Order = dto.Order
            };

            _context.Columns.Add(column);
            await _context.SaveChangesAsync();

            var columnDto = new ColumnDto
            {
                Id = column.Id,
                Name = column.Name,
                ProjectId = column.ProjectId,
                Order = column.Order
            };

            return Ok(columnDto);
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateColumn(int id, [FromBody] UpdateColumnDto dto)
        {
            var column = await _context.Columns.FindAsync(id);
            if (column == null) return NotFound("Колонку не знайдено.");
            column.Name = dto.Name;
            column.Order = dto.Order;

            _context.Columns.Update(column);
            await _context.SaveChangesAsync();
            return NoContent();
        }

    }
}
