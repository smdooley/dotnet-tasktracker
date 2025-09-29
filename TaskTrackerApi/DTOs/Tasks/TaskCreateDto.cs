using System.ComponentModel.DataAnnotations;

namespace TaskTrackerApi.DTOs.Tasks
{
    public class TaskCreateDto
    {
        [Required]
        [StringLength(200, MinimumLength = 1)]
        public string Title { get; set; } = string.Empty;

        [StringLength(1000)]
        public string? Description { get; set; }

        public DateTime? DueDate { get; set; }
    }
}