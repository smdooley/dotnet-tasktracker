using System.ComponentModel.DataAnnotations;

namespace TaskTrackerApi.DTOs.Tasks
{
    public class TaskUpdateDto
    {
        [Required(ErrorMessage = "Title is required")]
        [StringLength(200, MinimumLength = 1, ErrorMessage = "Title must be between 1 and 200 characters")]
        public string Title { get; set; } = string.Empty;

        [StringLength(1000, ErrorMessage = "Description must be a maximum of 1000 characters")]
        public string? Description { get; set; }

        public DateTime? DueDate { get; set; }

        public bool IsCompleted { get; set; }
    }
}