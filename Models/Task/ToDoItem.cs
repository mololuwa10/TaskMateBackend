using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text.Json.Serialization;
using System.Threading.Tasks;
using Backend.Models.Category;

namespace Backend.Models.Task
{
    public class ToDoItem
    {
        [Key]
        public int TaskId { get; set; }

        [Required]
        public string? TaskName { get; set; }

        public string? TaskDescription { get; set; }

        public DateTime DateCreated { get; set; }

        // = DateTime.UtcNow;
        public DateTime? DueDate { get; set; }

        public string? Priority { get; set; } // e.g., Low, Medium, High

        public bool? IsCompleted { get; set; }

        [JsonIgnore]
        public ICollection<SubTasks>? Subtasks { get; set; }

        [JsonIgnore]
        public Reccurence? Recurrence { get; set; }

        [JsonIgnore]
        public ICollection<Attachment>? Attachments { get; set; }

        // Foreign key for User
        public string? UserId { get; set; }

        // Navigation property for related user
        public User? User { get; set; }
        public int? CategoryId { get; set; }
        public Models.Category.Category? TaskCategory { get; set; } // e.g., Work, Personal
    }
}
