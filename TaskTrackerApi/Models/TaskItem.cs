using System.ComponentModel.DataAnnotations;

namespace TaskTrackerApi.Models
{
    public class TaskItem
    {
        public int Id { get; set; }

        [Required]
        [StringLength(200, MinimumLength = 1)]
        public string Title { get; set; } = string.Empty;

        [StringLength(1000)]
        public string Description { get; set; } = string.Empty;
        public DateTime? DueDate { get; set; } = null;
        public bool IsCompleted { get; set; } = false;
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

        // Foreign key to associate task with a user
        [Required]
        public int UserId { get; set; }
        public virtual User? User { get; set; } // Navigation property
    }
}