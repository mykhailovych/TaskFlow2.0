using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TaskFlow.Server.Data;
using TaskFlow.Server.Models;
using TaskFlow.Shared.DTOs;

namespace TaskFlow.Server.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class ProjectsController : ControllerBase
    {
        private readonly AppDbContext _context;

        public ProjectsController(AppDbContext context)
        {
            _context = context;
        }

        [HttpPost]
        public async Task<ActionResult<ProjectDto>> CreateProject([FromBody] CreateProjectDto dto)
        {
            var owner = await _context.Users.FindAsync(dto.OwnerId);
            if (owner == null) return NotFound("Власника проєкту не знайдено");

            var project = new Project
            {
                Name = dto.Name,
                Description = dto.Description,
                OwnerId = dto.OwnerId,
                CreatedAt = DateTime.UtcNow

            };

            _context.Projects.Add(project);
            await _context.SaveChangesAsync();

            var defaultColumns = new List<Column>
            {
                new Column {Name = "To Do", Order = 1, ProjectId = project.Id },
                new Column {Name = "In Progress", Order = 2, ProjectId = project.Id },
                new Column {Name = "Done", Order = 3, ProjectId = project.Id }

            };

            _context.Columns.AddRange(defaultColumns);
            await _context.SaveChangesAsync();

            return Ok(new ProjectDto
            {
                Id = project.Id,
                Name = project.Name,
                Description = project.Description,
                CreatedAt = project.CreatedAt,
                OwnerId = project.OwnerId
            });

        }

        [HttpGet("user/{userId}")]
        public async Task<ActionResult<List<ProjectDto>>> GetUserProjects(int userId)
        {
            var projects = await _context.Projects.Where(p => p.OwnerId == userId).Select(p => new ProjectDto
            {
                Id = p.Id,
                Name = p.Name,
                Description = p.Description,
                CreatedAt = p.CreatedAt,
                OwnerId = p.OwnerId
            }).ToListAsync();
            return Ok(projects);
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteProject(int id)
        {
            var project = await _context.Projects.FindAsync(id);
            if (project == null) return NotFound("Проєкт не знайдено.");
            _context.Projects.Remove(project);
            await _context.SaveChangesAsync();
            return NoContent();
        }


        [HttpGet("{projectId}/board")]
        public async Task<ActionResult<List<ColumnDto>>> GetProjectBoard(int projectId)
        {
            var columns = await _context.Columns
                .Where(c => c.ProjectId == projectId)
                .OrderBy(c => c.Order)
                .Select(c => new ColumnDto
                {
                    Id = c.Id,
                    Name = c.Name,
                    Order = c.Order,
                    ProjectId = c.ProjectId,

                    Tasks = _context.Tasks
                        .Where(t => t.ColumnId == c.Id)
                        .OrderBy(t => t.Order)
                        .Select(t => new TaskDto
                        {
                            Id = t.Id,
                            Title = t.Title,
                            Description = t.Description,
                            ColumnId = t.ColumnId,
                            Order = t.Order,
                            Priority = t.Priority,
                            Deadline = t.Deadline,
                            CreatedAt = t.CreatedAt,

                            // 🛡️ ЗАЛІЗОБЕТОННИЙ СПОСІБ: Прямий вибіг із таблиці Comments за TaskId
                            Comments = _context.Comments
                                .Where(cmt => cmt.TaskId == t.Id)
                                .OrderBy(cmt => cmt.CreatedAt)
                                .Select(cmt => new CommentDto
                                {
                                    Id = cmt.Id,
                                    Text = cmt.Text,
                                    CreatedAt = cmt.CreatedAt
                                })
                                .ToList()
                        })
                        .ToList()
                })
                .ToListAsync();

            return Ok(columns);
        }

    }
}
