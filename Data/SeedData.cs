using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Backend.Models.Category;
using Microsoft.EntityFrameworkCore;

namespace Backend.Data
{
	public static class SeedData
	{
		public static void Initialize(IServiceProvider serviceProvider)
		{
			using var context = new ApplicationDbContext(
				serviceProvider.GetRequiredService<DbContextOptions<ApplicationDbContext>>()
			);

			if (context.Categories.Any())
			{
				return;
			}

			context.Categories.AddRange(
				new Category { CategoryName = "Work" },
				new Category { CategoryName = "Personal" },
				new Category { CategoryName = "Health" },
				new Category { CategoryName = "Shopping" },
				new Category { CategoryName = "Fitness" },
				new Category { CategoryName = "Education" },
				new Category { CategoryName = "Finance" },
				new Category { CategoryName = "Social" },
				new Category { CategoryName = "Family" },
				new Category { CategoryName = "Hobbies" },
				new Category { CategoryName = "Projects" },
				new Category { CategoryName = "Self-Care" },
				new Category { CategoryName = "Errands" },
				new Category { CategoryName = "Goals" }
			);

			context.SaveChanges();
		}
	}
}
