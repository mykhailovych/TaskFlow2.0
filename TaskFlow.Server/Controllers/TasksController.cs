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
                Deadline = dto.Deadline,
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
            var taskItem = await _context.Tasks
                .Include(t => t.Comments)
                .FirstOrDefaultAsync(t => t.Id == id);

            if (taskItem == null)
            {
                return NotFound("Завдання не знайдено.");
            }

            // 2. Оновлюємо основні поля
            taskItem.Title = dto.Title;
            taskItem.Description = dto.Description;
            taskItem.ColumnId = dto.ColumnId;
            taskItem.Order = dto.Order;
            taskItem.Priority = dto.Priority ?? "Medium";
            taskItem.Deadline = dto.Deadline;

            await _context.SaveChangesAsync();

            return Ok(new TaskDto
            {
                Id = taskItem.Id,
                Title = taskItem.Title,
                Description = taskItem.Description,
                ColumnId = taskItem.ColumnId,
                Order = taskItem.Order,
                Priority = taskItem.Priority,
                Deadline = taskItem.Deadline,
                CreatedAt = taskItem.CreatedAt,


                Comments = taskItem.Comments.Select(c => new CommentDto
                {
                    Id = c.Id,
                    Text = c.Text,
                    CreatedAt = c.CreatedAt
                }).ToList()
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

        [HttpPost("{id}/comments")]
        public async Task<IActionResult> AddComment(int id, [FromBody] CreateCommentDto dto)
        {
            var task = await _context.Tasks.FindAsync(id);
            if (task == null) return NotFound("Завдання не знадено");

            var comment = new Comment
            {
                Text = dto.Text,
                CreatedAt = DateTime.Now,
                TaskId = id
            };

            _context.Comments.Add(comment);
            await _context.SaveChangesAsync();

            return Ok(new CommentDto
            {
                Id = comment.Id,
                Text = comment.Text,
                CreatedAt = comment.CreatedAt
            });
        }
    }
}