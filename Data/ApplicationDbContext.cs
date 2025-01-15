using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Backend.Models;
using Backend.Models.Category;
using Backend.Models.Task;
using Microsoft.EntityFrameworkCore;

namespace Backend.Data
{
	public class ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : DbContext(options)
	{
        public DbSet<ToDoItem>? ToDoItems { get; set; }
		public DbSet<Category>? Categories { get; set; }

		public DbSet<Reccurence>? Reccurences { get; set; }

		public DbSet<Attachment>? Attachments { get; set; }

		public DbSet<SubTasks>? SubTasks { get; set; }

		public DbSet<User>? Users { get; set; }

		protected override void OnModelCreating(ModelBuilder modelBuilder)
		{
			base.OnModelCreating(modelBuilder);
		}
	}
}
