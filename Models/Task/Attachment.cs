using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace Backend.Models.Task
{
	public class Attachment
	{
		[Key]
		public int AttachmentId { get; set; }
		
		public string? AttachmentName { get; set; } // Name of attachment
		
		public string? AttachmentPath { get; set; } // Path or URL to the attachment
		
		public string? AttachmentType { get; set; } // Type of attachment e.g., file, image
		public int TaskId { get; set; } // Foreign key to ToDoItem		
		public ToDoItem? Task { get; set; }
	}
}