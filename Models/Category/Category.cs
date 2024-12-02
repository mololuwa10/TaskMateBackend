using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace Backend.Models.Category
{
	public class Category
	{
		[Key]
		public int? CategoryId { get; set; }
		
		[Required]
		public string? CategoryName { get; set; }
		
		public string? UserId { get; set; }
		
		public DateTime? DateCreated { get; set; }
		
		public User? User { get; set; }
	}
}