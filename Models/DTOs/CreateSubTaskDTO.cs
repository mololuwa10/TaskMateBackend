using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Backend.Models.DTOs
{
    public class CreateSubTaskDTO
    {
        public string? SubTaskName { get; set; }
        public string? SubtaskDescription { get; set; }
        public DateTime? SubtaskDueDate { get; set; }
        public bool SubtaskIsCompleted { get; set; }
        public int TaskId { get; set; }
    }
}
