using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using Backend.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Models.DTOs;
using Models.Authorization;
using Backend.Data;
using Microsoft.EntityFrameworkCore;

namespace Controllers.AuthController
{
	[ApiController]
	[Route("api/[controller]")]
	public class UserAccountController(UserManager<User>? userManager, ApplicationDbContext? dbContext) : ControllerBase
	{
		
		[HttpPut("edit-user")]
		[Authorize]
		public async Task<IActionResult> EditUser([FromBody] EditUserModel editUserModel) 
		{
			var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
			if (string.IsNullOrEmpty(userId)) 
			{
				return Unauthorized("User must be authenticated to edit their account.");
			}
			
			var user = await userManager!.FindByIdAsync(userId);
			if (user == null) 
			{
				return NotFound("User not found.");
			}
			
			user.FirstName = editUserModel.FirstName ?? user.FirstName;
			user.LastName = editUserModel.LastName ?? user.LastName;
			user.Email = editUserModel.Email ?? user.Email;
			user.PhoneNumber = editUserModel.PhoneNumber ?? user.PhoneNumber;
			user.UserName = editUserModel.Username ?? user.UserName;
			
			user.DateModified = DateTime.UtcNow;
			
			var updateResult = await userManager.UpdateAsync(user);
			if (!updateResult.Succeeded) 
			{
				return BadRequest("Failed to update user.");
			}
			
			return Ok(new {user.FirstName, user.LastName, user.Email, user.UserName, user.PhoneNumber, user.DateCreated, user.DateModified, message = "User updated successfully"});
		}
		
		[HttpDelete("delete-user")]
		[Authorize]
		public async Task<IActionResult> DeleteUser() 
		{
			var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
			if (string.IsNullOrEmpty(userId))
			{
				return Unauthorized("User must be authenticated to delete their account.");
			}
			
			var user = await userManager!.FindByIdAsync(userId);
			if (user == null)
			{
				return NotFound("User not found.");
			}
			
			var toDoItems = dbContext?.ToDoItems?
				.Where(item => item.UserId == userId)
				.Include(item => item.Subtasks)
				.Include(item => item.Recurrence)
				.Include(item => item.Attachments);
			
			foreach (var item in toDoItems!)
			{
				if (item.Subtasks != null) 
				{
					dbContext?.SubTasks?.RemoveRange(item.Subtasks);
				}
				
				if (item.Recurrence != null) 
				{
					dbContext?.Reccurences?.Remove(item.Recurrence);
				}
				
				if (item.Attachments != null) 
				{
					dbContext?.Attachments?.RemoveRange(item.Attachments);
				}
				
				dbContext?.ToDoItems?.Remove(item);
			}
			
			await dbContext!.SaveChangesAsync();
					
			var deleteResult = await userManager.DeleteAsync(user);
			if (!deleteResult.Succeeded)
			{
				return BadRequest("Failed to delete user.");
			}

			return Ok(new {message = "User and all related data deleted successfully"});
		}
		
		[HttpPut("change-password")]
		[Authorize]
		public async Task<IActionResult> ChangePassword([FromBody] ChangePasswordModel changePasswordModel) 
		{
			var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
			if (string.IsNullOrEmpty(userId))
			{
				return Unauthorized("User must be authenticated to change their password.");
			}
			
			var user = await userManager!.FindByIdAsync(userId);
			if (user == null)
			{
				return NotFound("User not found.");
			}
			
			// Checking if the new password has been entered without providing the old password
			if (!string.IsNullOrEmpty(changePasswordModel.NewPassword) && string.IsNullOrEmpty(changePasswordModel.OldPassword)) 
			{
				return BadRequest("Old password is required to change the password.");
			}
			
			// Validate the old password
			if (string.IsNullOrEmpty(changePasswordModel.OldPassword)) 
			{
				var passwordCheck = await userManager.CheckPasswordAsync(user, changePasswordModel.OldPassword!);
				if (!passwordCheck) 
				{
					return BadRequest("Invalid old password.");
				}
				
				if (changePasswordModel.OldPassword == changePasswordModel.NewPassword) 
				{
					return BadRequest("New password cannot be the same as the old password.");
				}
			}
			
			// Checking if the new password and confirm new password match
			if (!string.IsNullOrEmpty(changePasswordModel.NewPassword) && changePasswordModel.NewPassword != changePasswordModel.ConfirmNewPassword) 
			{
				return BadRequest("Passwords do not match.");
			}
			
			// If the old password is provided, update the password
			if (!string.IsNullOrEmpty(changePasswordModel.OldPassword) && !string.IsNullOrEmpty(changePasswordModel.NewPassword))
			{
				var token = await userManager.GeneratePasswordResetTokenAsync(user);
				var resetResult = await userManager.ResetPasswordAsync(user, token, changePasswordModel.NewPassword);

				if (!resetResult.Succeeded)
				{
					return BadRequest("Failed to update password.");
				}
			}
			
			user.DateModified = DateTime.UtcNow;	
			
			var updateResult = await userManager.UpdateAsync(user);

			if (!updateResult.Succeeded) 
			{
				return BadRequest("Failed to update password");
			}

			return Ok(new {user.DateModified, message = "Password updated successfully"});
		}
	}
}