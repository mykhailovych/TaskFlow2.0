using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TaskFlow.Server.Data;
using TaskFlow.Server.Models;
using TaskFlow.Shared.DTOs;

namespace TaskFlow.Server.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class TasksController : ControllerBase
    {
        private readonly AppDbContext _context;

        public TasksController(AppDbContext context)
        {
            _context = context;
        }


        [HttpPost]
        public async Task<ActionResult<TaskDto>> CreateTask([FromBody] CreateTaskDto dto)
        {
            var column = await _context.Columns.FindAsync(dto.ColumnId);
            if (column == null)
            {
                return NotFound("Колонку не знайдено.");
            }
            int currentMaxOrder = await _context.Tasks
                .Where(t => t.ColumnId == dto.ColumnId)
                .Select(t => (int?)t.Order)
                .MaxAsync() ?? 0;

            var taskItem = new TaskItem
            {
                Title = dto.Title,
                Description = dto.Description,
                ColumnId = dto.ColumnId,
                Order = currentMaxOrder + 1,
                Priority = dto.Priority,
                CreatedAt = DateTime.UtcNow
            };

            _context.Tasks.Add(taskItem);
            await _context.SaveChangesAsync();

            return Ok(new TaskDto
            {
                Id = taskItem.Id,
                Title = taskItem.Title,
                Description = taskItem.Description,
                ColumnId = taskItem.ColumnId,
                Order = taskItem.Order,
                Priority = taskItem.Priority,
                CreatedAt = taskItem.CreatedAt
            });
        }

        [HttpPut("{id}")]
        public async Task<ActionResult<TaskDto>> UpdateTask(int id, [FromBody] UpdateTaskDto dto)
        {
            var taskItem = await _context.Tasks.FindAsync(id);
            if (taskItem == null)
            {
                return NotFound("Завдання не знайдено.");
            }

            taskItem.Title = dto.Title;
            taskItem.Description = dto.Description;
            taskItem.ColumnId = dto.ColumnId;
            taskItem.Order = dto.Order;
            taskItem.Priority = dto.Priority;

            await _context.SaveChangesAsync();

            return Ok(new TaskDto
            {
                Id = taskItem.Id,
                Title = taskItem.Title,
                Description = taskItem.Description,
                ColumnId = taskItem.ColumnId,
                Order = taskItem.Order,
                Priority = taskItem.Priority,
                CreatedAt = taskItem.CreatedAt
            });
        }
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteTask(int id)
        {
            var taskItem = await _context.Tasks.FindAsync(id);
            if (taskItem == null)
            {
                return NotFound("Завдання не знайдено.");
            }

            _context.Tasks.Remove(taskItem);
            await _context.SaveChangesAsync();

            return NoContent();
        }
    }
}