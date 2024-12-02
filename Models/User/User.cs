using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;
using Backend.Models.Task;
using Microsoft.AspNetCore.Identity;

namespace Backend.Models
{
    public class User : IdentityUser
    {
        public string? FirstName { get; set; }

        public string? LastName { get; set; }

        // [Required]
        // [MaxLength(50)]
        // public string? Username { get; set; }

        // [Required]
        // [MaxLength(100)]
        // public string? Email { get; set; }

        // [Required]
        // public string? PasswordHash { get; set; } // Hashed password

        public DateTime? DateCreated { get; set; } = DateTime.UtcNow;

        public ICollection<ToDoItem>? ToDoItems { get; set; }
    }
}
