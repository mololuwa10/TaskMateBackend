using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using Backend.Data;
using Backend.Models;
using Backend.Models.Category;
using Backend.Models.DTOs;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Backend.Controllers.CategoryController
{
	[ApiController]
	[Route("api/[controller]")]
	public class CategoriesController(ApplicationDbContext dbContext, UserManager<User> userManager)
		: ControllerBase
	{
		private readonly ApplicationDbContext dbContext = dbContext;
		private readonly UserManager<User> userManager = userManager;

		[HttpGet]
		public async Task<ActionResult<IEnumerable<Category>>> GetCategories()
		{
			// var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
			var categories = await (dbContext.Categories?.ToListAsync() ?? Task.FromResult(new List<Category>()));

			return Ok(categories);
		}

		[HttpPost]
		[Authorize]
		public async Task<ActionResult<Category>> CreateCategory([FromBody] CategoryDTO categoryDTO)
		{
			if (!ModelState.IsValid)
			{
				return BadRequest(ModelState);
			}

			var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
			var category = new Category
			{
				CategoryName = categoryDTO.CategoryName,
				UserId = userId,
				DateCreated = DateTime.UtcNow
			};

			dbContext?.Categories?.Add(category);
			if (dbContext == null) return BadRequest("Database is null");
			await dbContext.SaveChangesAsync();

			return CreatedAtAction(
				nameof(GetCategories),
				new { id = category.CategoryId },
				category
			);
		}
	}
}
