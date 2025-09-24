using System.ComponentModel.DataAnnotations;

namespace TaskTrackerApi.Models
{
    /// <summary>
    /// Represents a user in the TaskTracker system.
    /// </summary>
    public class User
    {
        public int Id { get; set; }

        [Required]
        [StringLength(50, MinimumLength = 3)]
        public string Username { get; set; } = string.Empty;

        [Required]
        public string PasswordHash { get; set; } = string.Empty;
        
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        // Navigation property for related tasks
        public ICollection<TaskItem> Tasks { get; set; } = new List<TaskItem>();
    }
}