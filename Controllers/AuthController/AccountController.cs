using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using Backend.Data;
using Backend.Models;
using Backend.Models.Authentication;
using Backend.Models.Token;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

namespace Backend.Controllers.AuthController
{
	[ApiController]
	[Route("api/[controller]")]
	public class AccountController : ControllerBase
	{
		private readonly UserManager<User>? userManager;
		private readonly SignInManager<User>? signInManager;
		private readonly IConfiguration? configuration;

		public AccountController(
			UserManager<User> userManager,
			SignInManager<User> signInManager,
			IConfiguration configuration
		)
		{
			this.userManager = userManager;
			this.signInManager = signInManager;
			this.configuration = configuration;
		}

		[HttpPost("register")]
		public async Task<IActionResult> Register([FromBody] RegisterModel model)
		{
			if (ModelState.IsValid)
			{
				#pragma warning disable
				var existingUser = await userManager?.FindByEmailAsync(model.Email ?? string.Empty);

				if (existingUser != null)
				{
					ModelState.AddModelError("Email", "The email address is already in use.");
					return BadRequest(ModelState);
				}
				var user = new User
				{
					FirstName = model.FirstName,
					LastName = model.LastName,
					UserName = model.Username,
					Email = model.Email,
					DateCreated = DateTime.UtcNow.ToUtc()
				};
				var result = await userManager!.CreateAsync(user, model.Password ?? string.Empty);

				if (result.Succeeded)
				{
					var token = JwtTokenGenerator.GenerateToken(
						user.Id.ToString(),
						user.UserName ?? string.Empty,
						configuration!
					);
					return Ok(new { Token = token, message = "User registered successfully" });
				}

				foreach (var error in result.Errors)
				{
					ModelState.AddModelError(string.Empty, error.Description);
				}
			}

			return BadRequest(ModelState);
		}

		[HttpPost("login")]
		public async Task<IActionResult> Login([FromBody] LoginModel model)
		{
			if (ModelState.IsValid)
			{
				User? user = null;

				if (new EmailAddressAttribute().IsValid(model.UsernameOrEmail))
				{
					user = await userManager!.FindByEmailAsync(
						model.UsernameOrEmail ?? string.Empty
					);
				}
				else
				{
					user = await userManager!.FindByNameAsync(
						model.UsernameOrEmail ?? string.Empty
					);
				}

				if (user != null)
				{
					var result = await signInManager!.CheckPasswordSignInAsync(
						user,
						model.Password ?? string.Empty,
						false
					);

					if (result.Succeeded)
					{
						var token = JwtTokenGenerator.GenerateToken(
							user.Id.ToString(),
							user.UserName ?? string.Empty,
							configuration!
						);
						return Ok(new { Token = token, message = "Login successful" });
					}
				}

				ModelState.AddModelError(string.Empty, "Invalid Login attempt");
			}

			return BadRequest(ModelState);
		}

		[HttpGet("details")]
		[Authorize]
		public async Task<IActionResult> GetUserDetails()
		{
			var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
			if (string.IsNullOrEmpty(userId))
			{
				return Unauthorized("User not authenticated");
			}

			var user = await userManager!.FindByIdAsync(userId);
			if (user == null)
			{
				return NotFound("User not found");
			}

			// Return all user details
			return Ok(
				new
				{
					Id = user.Id,
					FirstName = user.FirstName,
					LastName = user.LastName,
					UserName = user.UserName,
					Email = user.Email,
					DateCreated = user.DateCreated,
					ToDoItems = user.ToDoItems,
					PhoneNumber = user.PhoneNumber,
					PhoneNumberConfirmed = user.PhoneNumberConfirmed,
					TwoFactorEnabled = user.TwoFactorEnabled,
					LockoutEnabled = user.LockoutEnabled,
					LockoutEnd = user.LockoutEnd,
					AccessFailedCount = user.AccessFailedCount,
					ConcurrencyStamp = user.ConcurrencyStamp,
					SecurityStamp = user.SecurityStamp,
					EmailConfirmed = user.EmailConfirmed,
				}
			);
		}
		

		[HttpPost("logout")]
		[Authorize]
		public async Task<IActionResult> Logout()
		{
			await signInManager!.SignOutAsync();
			return Ok("Logout successful");
		}
	}
}
