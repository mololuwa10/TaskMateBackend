using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using Backend.Data;
using Backend.Models;
using Backend.Models.Category;
using Backend.Models.DTOs;
using Backend.Models.Task;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;

#pragma warning disable CS8602 // Dereference of a possibly null reference.

namespace Backend.Controllers.TaskController
{
	[ApiController]
	[Authorize]
	[Route("/api/todoitems")]
	public class ToDoController(ApplicationDbContext context) : ControllerBase
	{
		// Get Todos
		[HttpGet]
		public async Task<ActionResult<IEnumerable<ToDoItemsDTO>>> GetToDoItems()
		{
			var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
			var items = await context?
				.ToDoItems?.Where(t => t.UserId == userId)
				.Select(t => new ToDoItemsDTO
				{
					TaskId = t.TaskId,
					TaskName = t.TaskName,
					TaskDescription = t.TaskDescription,
					DueDate = t.DueDate.HasValue ? t.DueDate.Value : DateTime.MinValue,
					Priority = t.Priority,
					IsCompleted = t.IsCompleted,
					CategoryId = t.CategoryId.HasValue ? (int)t.CategoryId : 0,
					DateCreated = t.DateCreated,
					UserId = t.UserId,
					Subtasks = t.Subtasks,
					Recurrence = t.Recurrence,
					Attachments = t.Attachments,
				})
				.ToListAsync();

			return Ok(items);
		}

		[HttpGet("{id}")]
		public async Task<ActionResult<ToDoItem>> GetToDoItem(int id)
		{
			var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
			var item = await context?
				.ToDoItems?.Where(t => t.UserId == userId && t.TaskId == id)
				.Select(t => new ToDoItemsDTO
				{
					TaskId = t.TaskId,
					TaskName = t.TaskName,
					TaskDescription = t.TaskDescription,
					DueDate = t.DueDate.HasValue ? t.DueDate.Value : DateTime.MinValue,
					Priority = t.Priority,
					IsCompleted = t.IsCompleted,
					CategoryId = t.CategoryId.HasValue ? (int)t.CategoryId : 0,
					CategoryName = t.TaskCategory != null ? t.TaskCategory.CategoryName : null,
					DateCreated = t.DateCreated,
					UserId = t.UserId,
					Subtasks = t.Subtasks,
					Recurrence = t.Recurrence,
					Attachments = t.Attachments,
				})
				.FirstOrDefaultAsync();

			if (item == null)
			{
				return NotFound();
			}

			return Ok(item);
		}

		// Get In-Progress Tasks (IsCompleted = false)
		[HttpGet("in-progress")]
		public async Task<ActionResult<IEnumerable<ToDoItemsDTO>>> GetInProgressTasks()
		{
			var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
			if (string.IsNullOrEmpty(userId))
			{
				return Unauthorized("User must be logged in");
			}
			var itemsQuery = context?.ToDoItems?.Where(t =>
				t.UserId == userId && t.IsCompleted == false
			);

			var count = await itemsQuery?.CountAsync();
			var items = await itemsQuery
				.Select(t => new ToDoItemsDTO
				{
					TaskId = t.TaskId,
					TaskName = t.TaskName,
					TaskDescription = t.TaskDescription,
					DueDate = t.DueDate.HasValue ? t.DueDate.Value : DateTime.MinValue,
					Priority = t.Priority,
					IsCompleted = t.IsCompleted,
					CategoryId = t.CategoryId.HasValue ? (int)t.CategoryId : 0,
					DateCreated = t.DateCreated,
					UserId = t.UserId,
					Subtasks = t.Subtasks,
					Recurrence = t.Recurrence,
					Attachments = t.Attachments,
				})
				.ToListAsync();

			return Ok(new { Count = count, Items = items });
		}

		// Get Completed Tasks (IsCompleted = true)
		[HttpGet("completed")]
		public async Task<ActionResult<IEnumerable<ToDoItemsDTO>>> GetCompletedTasks()
		{
			var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
			if (string.IsNullOrEmpty(userId))
			{
				return Unauthorized("User must be logged in");
			}
			var itemsQuery = context?.ToDoItems?.Where(t =>
				t.UserId == userId && t.IsCompleted == true
			);

			var count = await itemsQuery?.CountAsync();
			var items = await itemsQuery
				.Select(t => new ToDoItemsDTO
				{
					TaskId = t.TaskId,
					TaskName = t.TaskName,
					TaskDescription = t.TaskDescription,
					DueDate = t.DueDate.HasValue ? t.DueDate.Value : DateTime.MinValue,
					Priority = t.Priority,
					IsCompleted = t.IsCompleted,
					CategoryId = t.CategoryId.HasValue ? (int)t.CategoryId : 0,
					DateCreated = t.DateCreated,
					UserId = t.UserId,
					Subtasks = t.Subtasks,
					Recurrence = t.Recurrence,
					Attachments = t.Attachments,
				})
				.ToListAsync();

			return Ok(new { Count = count, Items = items });
		}

		// POST: api/todo
		[HttpPost]
		public async Task<ActionResult<ToDoItem>> PostTodoItem(
			[FromForm] string ToDoItem,
			[FromForm] List<IFormFile> attachments
		)
		{
			var createToDoItemsDTO = JsonConvert.DeserializeObject<CreateToDoItemsDTO>(ToDoItem);

			var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
			int? categoryId = createToDoItemsDTO?.CategoryId;

			if (categoryId == null && !string.IsNullOrEmpty(createToDoItemsDTO?.CategoryName))
			{
				// Check if the category exists globally
				var existingCategory = await context?.Categories?.FirstOrDefaultAsync(c =>
					c.CategoryName == createToDoItemsDTO.CategoryName
				);

				if (existingCategory != null)
				{
					// If the category exists, use its CategoryId
					categoryId = existingCategory?.CategoryId;
				}
				else
				{
					// If the category does not exist, create a new one
					var newCategory = new Category
					{
						CategoryName = createToDoItemsDTO.CategoryName,
						UserId = userId,
						DateCreated = DateTime.UtcNow,
					};

					context.Categories.Add(newCategory);
					await context.SaveChangesAsync();

					categoryId = newCategory.CategoryId;
				}
			}

			var item = new ToDoItem
			{
				TaskName = createToDoItemsDTO?.TaskName,
				TaskDescription = createToDoItemsDTO?.TaskDescription,
				DateCreated = createToDoItemsDTO?.DateCreated ?? DateTime.UtcNow,
				DueDate = createToDoItemsDTO?.DueDate,
				Priority = createToDoItemsDTO?.Priority,
				IsCompleted = false,
				UserId = userId,
				CategoryId = categoryId,
			};

			context.ToDoItems.Add(item);
			await context.SaveChangesAsync();

			// Handle subtask
			if (createToDoItemsDTO?.Subtasks != null)
			{
				foreach (var subtaskDto in createToDoItemsDTO.Subtasks)
				{
					var subTask = new SubTasks
					{
						SubTaskName = subtaskDto.SubTaskName,
						SubtaskDescription = subtaskDto.SubtaskDescription,
						SubtaskDueDate = subtaskDto.SubtaskDueDate,
						SubtaskIsCompleted = false,
						TaskId = item.TaskId,
					};
					context.SubTasks.Add(subTask);
				}
			}

			// Handle Reccurence
			if (createToDoItemsDTO?.Recurrence != null)
			{
				var recurrence = new Reccurence
				{
					Interval = createToDoItemsDTO.Recurrence.Interval,
					TaskId = item.TaskId,
				};
				context.Reccurences.Add(recurrence);
			}

			// Handle Attachments
			if (attachments != null && attachments.Any())
			{
				foreach (var file in attachments)
				{
					if (file.Length > 0)
					{
						// Define a path to save the file
						var filePath = Path.Combine(
							"wwwroot",
							"uploads",
							Guid.NewGuid().ToString() + Path.GetExtension(file.FileName)
						);
						filePath = filePath.Replace("\\", "/");

						// Optionally, you may want to ensure the uploads directory exists
						var directoryPath = Path.GetDirectoryName(filePath);
						if (directoryPath != null && !Directory.Exists(directoryPath))
						{
							Directory.CreateDirectory(directoryPath);
						}

						// Save the file to the server
						using (var stream = new FileStream(filePath, FileMode.Create))
						{
							await file.CopyToAsync(stream);
						}

						var attachment = new Attachment
						{
							AttachmentName = file.FileName,
							AttachmentPath = filePath,
							AttachmentType = file.ContentType,
							TaskId = item.TaskId,
						};
						context.Attachments.Add(attachment);
					}
				}
			}

			await context.SaveChangesAsync();

			return CreatedAtAction(
				nameof(GetToDoItem),
				new { id = item.TaskId },
				new ToDoItemsDTO
				{
					TaskId = item.TaskId,
					TaskName = item.TaskName,
					TaskDescription = item.TaskDescription,
					DateCreated = item.DateCreated,
					DueDate = item.DueDate.HasValue ? item.DueDate.Value : DateTime.MinValue,
					Priority = item.Priority,
					IsCompleted = item.IsCompleted,
					UserId = userId,
					CategoryId = item.CategoryId.HasValue ? (int)item.CategoryId : 0,
					Subtasks = item.Subtasks,
					Recurrence = item.Recurrence,
					Attachments = item.Attachments,
				}
			);
		}

		[HttpPut("{id}")]
		public async Task<IActionResult> UpdateToDoItem(int id, [FromForm] string ToDoItem)
		{
			var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
			var updateToDoItemDTO = JsonConvert.DeserializeObject<UpdateToDoItemDTO>(ToDoItem);

			var toDoItem = await context?.ToDoItems?.FirstOrDefaultAsync(t =>
				t.TaskId == id && t.UserId == userId
			);

			if (toDoItem == null)
			{
				return NotFound();
			}

			// Update main task properties
			toDoItem.TaskName = updateToDoItemDTO?.TaskName;
			toDoItem.TaskDescription = updateToDoItemDTO?.TaskDescription;
			toDoItem.DateCreated = updateToDoItemDTO?.DateCreated ?? toDoItem.DateCreated;
			toDoItem.DueDate = updateToDoItemDTO?.DueDate;
			toDoItem.Priority = updateToDoItemDTO?.Priority;
			toDoItem.IsCompleted = updateToDoItemDTO?.IsCompleted ?? toDoItem.IsCompleted;
			toDoItem.CategoryId = updateToDoItemDTO?.CategoryId;

			// Update task in the database
			context.Entry(toDoItem).State = EntityState.Modified;
			await context.SaveChangesAsync();

			var updatedItemDTO = new ToDoItemsDTO
			{
				TaskId = toDoItem.TaskId,
				TaskName = toDoItem.TaskName,
				TaskDescription = toDoItem.TaskDescription,
				DateCreated = toDoItem.DateCreated,
				DueDate = toDoItem.DueDate.HasValue ? toDoItem.DueDate.Value : DateTime.MinValue,
				Priority = toDoItem.Priority,
				IsCompleted = toDoItem.IsCompleted,
				UserId = toDoItem.UserId,
				CategoryId = toDoItem.CategoryId.HasValue ? (int)toDoItem.CategoryId : 0,
				// Recurrence = toDoItem?.Recurrence,
				// Attachments = toDoItem?.Attachments,
			};

			return Ok(updatedItemDTO);
		}

		[HttpDelete("{id}")]
		public async Task<IActionResult> DeleteToDoItem(int id)
		{
			var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
			var toDoItem = await context?
				.ToDoItems?.Include(t => t.Subtasks)
				.Include(t => t.Recurrence)
				.Include(t => t.Attachments)
				.FirstOrDefaultAsync(t => t.TaskId == id && t.UserId == userId);

			if (toDoItem == null)
			{
				return NotFound();
			}

			if (
				toDoItem.Subtasks != null
				&& toDoItem.Recurrence != null
				&& toDoItem.Attachments != null
			)
			{
				context.SubTasks.RemoveRange(toDoItem.Subtasks);
				context.Reccurences.Remove(toDoItem.Recurrence);
				context.Attachments.RemoveRange(toDoItem.Attachments);
			}

			context.ToDoItems.Remove(toDoItem);

			await context.SaveChangesAsync();

			return NoContent();
		}

		// Mark Task as complete
		[HttpPut("complete/{taskId}")]
		public async Task<IActionResult> CompleteSubTask(int taskId)
		{
			var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
			if (string.IsNullOrEmpty(userId))
			{
				return Unauthorized("User must be logged in");
			}

			var task = await context.ToDoItems.FindAsync(taskId);
			if (task == null)
			{
				return NotFound();
			}

			task.IsCompleted = true;
			await context.SaveChangesAsync();
			return Ok(task);
		}

		[HttpPut("in-complete/{taskId}")]
		public async Task<IActionResult> InCompleteSubTask(int taskId)
		{
			var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
			if (string.IsNullOrEmpty(userId))
			{
				return Unauthorized("User must be logged in");
			}

			var task = await context.ToDoItems.FindAsync(taskId);
			if (task == null)
			{
				return NotFound();
			}
			
			task.IsCompleted = false;
			await context.SaveChangesAsync();
			return Ok(task);
		}
	}
}
