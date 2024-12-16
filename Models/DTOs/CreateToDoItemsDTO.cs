using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Backend.Models.Task;

namespace Backend.Models.DTOs
{
    public class CreateToDoItemsDTO
    {
        public string? TaskName { get; set; }
        public string? TaskDescription { get; set; }
        public DateTime? DateCreated { get; set; }
        public DateTime? DueDate { get; set; }
        public string? Priority { get; set; }
        public int? CategoryId { get; set; }
        public string? CategoryName { get; set; }
        public ICollection<SubTasks>? Subtasks { get; set; }

        public Reccurence? Recurrence { get; set; }

        public ICollection<Attachment>? Attachments { get; set; }
    }
}
