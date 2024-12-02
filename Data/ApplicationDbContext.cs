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
	public class ApplicationDbContext : DbContext
	{
		public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options) {}

		public DbSet<ToDoItem> ToDoItems { get; set; }
		public DbSet<Category> Categories { get; set; }
		
		public DbSet<Reccurence> Reccurences { get; set; }

		public DbSet<Attachment> Attachments { get; set; }

		public DbSet<SubTasks> SubTasks { get; set; }

		public DbSet<User> Users { get; set; }
		
		// public override int SaveChanges()
		// {
		// 	ConvertDatesToUtc();
		// 	return base.SaveChanges();
		// }

		// public override Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
		// {
		// 	ConvertDatesToUtc();
		// 	return base.SaveChangesAsync(cancellationToken);
		// }

		// private void ConvertDatesToUtc()
		// {
		// 	var entries = ChangeTracker.Entries()
		// 		.Where(e => e.Entity is User && (e.State == EntityState.Added || e.State == EntityState.Modified));

		// 	foreach (var entityEntry in entries)
		// 	{
		// 		var user = (User)entityEntry.Entity;
		// 		user.DateCreated = user.DateCreated?.ToUtc();
		// 	}
		// }
		
		protected override void OnModelCreating(ModelBuilder modelBuilder)
		{
			base.OnModelCreating(modelBuilder);
		}
	}
}