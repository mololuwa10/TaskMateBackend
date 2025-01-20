using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using Backend.Data;
using Backend.Models;
using Controllers.AuthController;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Models.DTOs;
using Moq;
using Xunit;

namespace Tests
{
	public class UserAccountControllerTests
	{
		private readonly Mock<UserManager<User>>? _mockUserManager;
		private readonly ApplicationDbContext? _applicationDbContext;
		private readonly UserAccountController? _controller;
		
		public UserAccountControllerTests() 
		{
			_mockUserManager = new Mock<UserManager<User>>(
				new Mock<IUserStore<User>>().Object,
				null!, null!, null!, null!, null!, null!, null!, null!
			);
			// Set up in-memory database
			var options = new DbContextOptionsBuilder<ApplicationDbContext>()
				.UseInMemoryDatabase(databaseName: "TestDatabase")
				.Options;
				
			_applicationDbContext = new ApplicationDbContext(options);
			
			_controller = new UserAccountController(
				_mockUserManager?.Object,
				_applicationDbContext
			);
		}
		
		[Fact]
		public async Task EditUser_ValidUser_ReturnsOk()
		{
			// Arrange
			var user = new User
			{
				Id = Guid.NewGuid().ToString(),
				FirstName = "John",
				LastName = "Doe",
				UserName = "johndoe",
				Email = "johndoe@example.com",
				DateCreated = DateTime.UtcNow
			};
			
			// Add user to in-memory database
			_applicationDbContext?.Users?.Add(user);
			await _applicationDbContext!.SaveChangesAsync();
			
			// Configure UserManager mock
			_mockUserManager!.Setup(um => um.FindByIdAsync(It.IsAny<string>()))
				.ReturnsAsync((string id) => id == user.Id ? user : null);
			
			_mockUserManager.Setup(um => um.UpdateAsync(It.IsAny<User>()))
				.ReturnsAsync(IdentityResult.Success);

			var claims = new List<Claim>
			{
				new Claim(ClaimTypes.NameIdentifier, user.Id.ToString())
			};
			
			var identity = new ClaimsIdentity(claims);
			var principal = new ClaimsPrincipal(identity);
			
			_controller!.ControllerContext = new ControllerContext
			{
				HttpContext = new DefaultHttpContext
				{
					User = principal
				}
			};
			
			// Act
			var result = await _controller.EditUser(new EditUserModel
			{
				FirstName = "Jane",
				LastName = "Doe",
				Username = "janedoe",
				Email = "janedoe@example.com",
				PhoneNumber = "1234567890"
			});
			
			// Assert
			Assert.IsType<OkObjectResult>(result);
			
			// Verify user data was updated
			var updatedUser = await _applicationDbContext!.Users!.FindAsync(user.Id);
			Assert.NotNull(updatedUser);
			Assert.Equal("Jane", updatedUser!.FirstName);
			Assert.Equal("janedoe", updatedUser.UserName);
		}
		
		[Fact]
		
		public async Task EditUser_InvalidUser_ReturnsBadRequest() 
		{
			var user = new User
			{
				Id = Guid.NewGuid().ToString(),
				FirstName = "John",
				LastName = "Doe",
				UserName = "johndoe",
				Email = "johndoe@example.com",
				DateCreated = DateTime.UtcNow
			};
			
			// Add user to in-memory database
			_applicationDbContext?.Users?.Add(user);
			await _applicationDbContext!.SaveChangesAsync();
			
			// Configure UserManager mock
			_mockUserManager!.Setup(um => um.FindByIdAsync(It.IsAny<string>()))
				.ReturnsAsync((string id) => id == user.Id ? user : null);
			
			_mockUserManager.Setup(um => um.UpdateAsync(It.IsAny<User>()))
				.ReturnsAsync(IdentityResult.Success);
			
			var claims = new List<Claim>
			{
				new Claim(ClaimTypes.NameIdentifier, user.Id.ToString())
			};
			
			var identity = new ClaimsIdentity(claims);
			var principal = new ClaimsPrincipal(identity);
			
			_controller!.ControllerContext = new ControllerContext
			{
				HttpContext = new DefaultHttpContext
				{
					User = principal
				}
			};
			
			// Act
			var result = await _controller.EditUser(new EditUserModel
			{
				FirstName = "Jane",
				LastName = "Doe",
				Username = "janedoe",
				Email = "janedoe@example.com",
				PhoneNumber = "1234567890"
			});
			
			// Assert
			Assert.IsType<BadRequestObjectResult>(result);
			
		}
	}
}