using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace Backend.Models.Task
{
	public class Reccurence
	{
		[Key]
		public int RecurrenceId { get; set; }

		public string? Interval { get; set; } // e.g., daily, weekly, monthly

		public int TaskId { get; set; } // Foreign key to ToDoItem
		public ToDoItem? Task { get; set; }
	}
}