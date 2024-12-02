using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using Backend.Data;
using Backend.Models.DTOs;
using Backend.Models.Task;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Backend.Controllers.TaskController
{
	[ApiController]
	[Route("api/subtasks")]
	public class SubTaskController : ControllerBase
	{
		private readonly ApplicationDbContext context;

		public SubTaskController(ApplicationDbContext context)
		{
			this.context = context;
		}

		// Get all subtasks by subtask id
		[HttpGet("{id}")]
		public async Task<ActionResult<SubTasks>> GetSubTask(int id)
		{
			var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
			if (string.IsNullOrEmpty(userId))
			{
				return Unauthorized("User must be logged in");
			}

			var subTask = await context
				.SubTasks.Include(s => s.Task) 
				.FirstOrDefaultAsync(s => s.SubTaskId == id);
				
			if (subTask == null)
			{
				return NotFound();
			}

			// Check if the user owns the task associated with the subtask
			if (subTask.Task == null || subTask.Task.UserId != userId)
			{
				return Forbid("You do not have permission to view this subtask.");
			}

			return Ok(subTask);
		}

		// Get Subtask by task Id
		[HttpGet("task/{taskId}")]
		public async Task<ActionResult<IEnumerable<SubTasks>>> GetSubTasks(int taskId)
		{
			var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
			if (string.IsNullOrEmpty(userId))
			{
				return Unauthorized("User must be logged in");
			}

			var taskExists = await context.ToDoItems.AnyAsync(t =>
				t.TaskId == taskId && t.UserId == userId
			);

			if (!taskExists)
			{
				return NotFound(
					"Task not found or you do not have permission to view its subtasks."
				);
			}

			var subTasks = await context
				.SubTasks.Include(s => s.Task)
				.Where(s => s.TaskId == taskId)
				.ToListAsync();
			return Ok(subTasks);
		}

		// Add Subtask
		[HttpPost]
		public async Task<ActionResult<SubTasks>> PostSubTask(CreateSubTaskDTO createSubTaskDTO)
		{
			var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
			if (string.IsNullOrEmpty(userId))
			{
				return Unauthorized("User must be logged in");
			}

			var task = await context.ToDoItems.FirstOrDefaultAsync(t =>
				t.TaskId == createSubTaskDTO.TaskId && t.UserId == userId
			);
			if (task == null)
			{
				return NotFound(
					"Task not found or you do not have permission to add a subtask to this task."
				);
			}
			var subTask = new SubTasks
			{
				SubTaskName = createSubTaskDTO.SubTaskName,
				SubtaskDescription = createSubTaskDTO.SubtaskDescription,
				SubtaskIsCompleted = false,
				SubtaskDueDate = createSubTaskDTO.SubtaskDueDate,
				TaskId = createSubTaskDTO.TaskId,
			};

			context.SubTasks.Add(subTask);
			await context.SaveChangesAsync();

			return CreatedAtAction(nameof(GetSubTasks), new { id = subTask.SubTaskId }, subTask);
		}

		// Update Subtask
		[HttpPut("{id}")]
		public async Task<IActionResult> PutSubTask(int id, UpdateSubTaskDTO updateSubTaskDTO)
		{
			var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
			if (string.IsNullOrEmpty(userId))
			{
				return Unauthorized("User must be logged in");
			}

			// var task = await context.ToDoItems.FirstOrDefaultAsync(t => t.TaskId == updateSubTaskDTO.TaskId && t.UserId == userId);
			// if (task == null)
			// {
			// 	return NotFound("Task not found or you do not have permission to update this subtask.");
			// }

			var subTask = await context.SubTasks.FindAsync(id);

			if (subTask == null)
			{
				return NotFound();
			}

			subTask.SubTaskName = updateSubTaskDTO.SubTaskName;
			subTask.SubtaskDescription = updateSubTaskDTO.SubtaskDescription;
			subTask.SubtaskDueDate = updateSubTaskDTO.SubtaskDueDate;
			subTask.SubtaskIsCompleted = updateSubTaskDTO.SubtaskIsCompleted;

			await context.SaveChangesAsync();

			return Ok(subTask);
		}

		// Delete Subtask
		[HttpDelete("{id}")]
		public async Task<IActionResult> DeleteSubTask(int id)
		{
			var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
			if (string.IsNullOrEmpty(userId))
			{
				return Unauthorized("User must be logged in");
			}

			var subTask = await context.SubTasks.FindAsync(id);
			if (subTask == null)
			{
				return NotFound();
			}

			context.SubTasks.Remove(subTask);
			await context.SaveChangesAsync();

			return NoContent();
		}
		
		// Mark subtask as complete
		[HttpPut("complete/{subTaskId}")]
		
		public async Task<IActionResult> CompleteSubTask(int subTaskId) 
		{
			var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
			if(string.IsNullOrEmpty(userId)) 
			{
				return Unauthorized("User must be logged in");
			}

			var subTask = await context.SubTasks.FindAsync(subTaskId);
			if(subTask == null) 
			{
				return NotFound();
			}
			
			subTask.SubtaskIsCompleted = true;
			await context.SaveChangesAsync();
			return Ok(subTask);
		}
		
		// Mark subtask as in-complete
		[HttpPut("in-complete/{subTaskId}")]
		public async Task<IActionResult> InCompleteSubTask(int subTaskId) 
		{
			var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
			if(string.IsNullOrEmpty(userId)) 
			{
				return Unauthorized("User must be logged in");
			}

			var subTask = await context.SubTasks.FindAsync(subTaskId);
			if(subTask == null) 
			{
				return NotFound();
			}
			
			subTask.SubtaskIsCompleted = false;
			await context.SaveChangesAsync();
			return Ok(subTask);
		}
	}
}
