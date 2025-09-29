using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TaskTrackerApi.Data;
using TaskTrackerApi.DTOs.Errors;
using TaskTrackerApi.DTOs.Tasks;
using TaskTrackerApi.Models;

namespace TaskTrackerApi.Controllers
{
    [ApiController]
    [Authorize]
    [Route("api/[controller]")]
    public class TaskController : ControllerBase
    {
        private readonly AppDbContext _context;
        private readonly IMapper _mapper;

        public TaskController(AppDbContext context, IMapper mapper)
        {
            _context = context;
            _mapper = mapper;
        }

        // GET: api/Task
        [HttpGet]
        public async Task<ActionResult<IEnumerable<TaskItem>>> GetTasks()
        {
            var userId = GetCurrentUserId();
            var tasks = await _context.TaskItems
                .Where(t => t.UserId == userId)
                .OrderByDescending(t => t.CreatedAt)
                .ToListAsync();
            var response = _mapper.Map<List<TaskResponseDto>>(tasks);

            return Ok(response);
        }

        // GET: api/task/{id}
        [HttpGet("{id}")]
        public async Task<ActionResult<TaskItem>> GetTask(int id)
        {
            var userId = GetCurrentUserId();
            var task = await _context.TaskItems
                .FirstOrDefaultAsync(t => t.Id == id && t.UserId == userId);

            if (task == null)
            {
                // return NotFound($"Task with ID {id} not found.");
                return NotFound(new ErrorResponse
                {
                    StatusCode = 404,
                    Message = $"Task with ID {id} not found.",
                    Details = $"No task found with ID {id} for the current user."
                });
            }

            var response = _mapper.Map<TaskResponseDto>(task);

            return Ok(response);
        }

        // POST: api/task
        [HttpPost]
        public async Task<ActionResult<TaskItem>> CreateTask(TaskCreateDto request)
        {
            var userId = GetCurrentUserId();

            var task = _mapper.Map<TaskItem>(request);
            task.UserId = userId;

            _context.TaskItems.Add(task);
            await _context.SaveChangesAsync();

            var response = _mapper.Map<TaskResponseDto>(task);

            return CreatedAtAction(nameof(GetTask), new { id = response.Id }, response);
        }

        // PUT: api/task/{id}
        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateTask(int id, TaskUpdateDto request)
        {
            var userId = GetCurrentUserId();
            var task = await _context.TaskItems
                .FirstOrDefaultAsync(t => t.Id == id && t.UserId == userId);

            if (task == null)
            {
                // return NotFound($"Task with ID {id} not found.");
                return NotFound(new ErrorResponse
                {
                    StatusCode = 404,
                    Message = $"Task with ID {id} not found.",
                    Details = $"No task found with ID {id} for the current user."
                });
            }

            // Update task properties
            task.Title = request.Title;
            task.Description = request.Description;
            task.DueDate = request.DueDate;
            task.IsCompleted = request.IsCompleted;
            task.UpdatedAt = DateTime.UtcNow;

            await _context.SaveChangesAsync();

            // Return 204 NoContent to indicate successful update
            return NoContent();
        }

        // DELETE: api/task/{id}
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteTask(int id)
        {
            var userId = GetCurrentUserId();
            var task = await _context.TaskItems
                .FirstOrDefaultAsync(t => t.Id == id && t.UserId == userId);

            if (task == null)
            {
                // return NotFound($"Task with ID {id} not found.");
                return NotFound(new ErrorResponse
                {
                    StatusCode = 404,
                    Message = $"Task with ID {id} not found.",
                    Details = $"No task found with ID {id} for the current user."
                });
            }

            _context.TaskItems.Remove(task);
            await _context.SaveChangesAsync();

            // Return 204 NoContent to indicate successful deletion
            return NoContent();
        }

        private int GetCurrentUserId()
        {
            var userIdClaim = User.Claims.FirstOrDefault(c => c.Type == "userId");
            if (userIdClaim == null)
            {
                throw new UnauthorizedAccessException("User ID claim not found.");
            }
            return int.Parse(userIdClaim.Value);
        }
    }
}