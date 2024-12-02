using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace Backend.Models.Task
{
	public class SubTasks
	{
		[Key]
		public int SubTaskId { get; set; }
		
		[Required]
		public string? SubTaskName { get; set; }
		
		public string? SubtaskDescription { get; set; }

		public DateTime? SubtaskDueDate { get; set; }

		public bool SubtaskIsCompleted { get; set; }

		public int TaskId { get; set; } // Foreign key to ToDoItem
		
		public ToDoItem? Task { get; set; } 
	}
}