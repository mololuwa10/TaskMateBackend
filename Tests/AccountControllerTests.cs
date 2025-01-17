using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using Backend.Controllers.AuthController;
using Backend.Models;
using Backend.Models.Authentication;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Moq;
using Xunit;

namespace Tests
{
	public class AccountControllerTests
	{
		private readonly Mock<UserManager<User>> _mockUserManager;
		private readonly Mock<SignInManager<User>> _mockSignInManager;
		private readonly Mock<IConfiguration> _mockConfiguration;
		private readonly AccountController _controller;

		public AccountControllerTests()
		{
			_mockUserManager = new Mock<UserManager<User>>(
				new Mock<IUserStore<User>>().Object,
				null!, null!, null!, null!, null!, null!, null!, null!
			);
			_mockSignInManager = new Mock<SignInManager<User>>(
				_mockUserManager.Object,
				new Mock<Microsoft.AspNetCore.Http.IHttpContextAccessor>().Object,
				new Mock<IUserClaimsPrincipalFactory<User>>().Object,
				null!, null!, null!, null!
			);
			_mockConfiguration = new Mock<IConfiguration>();

			_controller = new AccountController(
				_mockUserManager.Object,
				_mockSignInManager.Object,
				_mockConfiguration.Object
			);
		}

		[Fact]
		public async Task Register_ValidUser_ReturnsOk()
		{
			// Arrange
			var registerModel = new RegisterModel
			{
				FirstName = "John",
				LastName = "Doe",
				Username = "johndoe",
				Email = "johndoe@example.com",
				Password = "Password123!"
			};
			
			_mockUserManager
				.Setup(m => m.FindByEmailAsync(registerModel.Email))
				.ReturnsAsync((User?)null);
			_mockUserManager
				.Setup(m => m.CreateAsync(It.IsAny<User>(), registerModel.Password))
				.ReturnsAsync(IdentityResult.Success);
				
			// Mock the Jwt section
			var mockJwtSection = new Mock<IConfigurationSection>();
			mockJwtSection.Setup(s => s["Key"]).Returns("test-very-long-secret-key-that-is-at-least-16-bytes-long");
			mockJwtSection.Setup(s => s["Issuer"]).Returns("test-issuer");
			mockJwtSection.Setup(s => s["Audience"]).Returns("test-audience");
			mockJwtSection.Setup(s => s["ExpiresInMinutes"]).Returns("30");

			_mockConfiguration.Setup(c => c.GetSection("Jwt")).Returns(mockJwtSection.Object);

			// Act
			var result = await _controller.Register(registerModel);
		
			// Assert
			var okResult = Assert.IsType<OkObjectResult>(result);
			Assert.Contains("User registered successfully", okResult?.Value?.ToString());
		}

		[Fact]
		public async Task Login_InvalidCredentials_ReturnsBadRequest()
		{
			// Arrange
			var loginModel = new LoginModel
			{
				UsernameOrEmail = "invaliduser@example.com",
				Password = "WrongPassword!"
			};

			_mockUserManager
				.Setup(m => m.FindByEmailAsync(loginModel.UsernameOrEmail))
				.ReturnsAsync((User?)null);

			// Act
			var result = await _controller.Login(loginModel);

			// Assert
			var badRequestResult = Assert.IsType<BadRequestObjectResult>(result);
			Assert.Contains("Invalid Login attempt", badRequestResult?.Value?.ToString());
		}

		[Fact]
		public async Task GetUserDetails_AuthenticatedUser_ReturnsUserDetails()
		{
			// Arrange
			var user = new User
			{
				Id = Guid.NewGuid().ToString(),
				FirstName = "Jane",
				LastName = "Doe",
				UserName = "janedoe",
				Email = "janedoe@example.com",
				DateCreated = DateTime.UtcNow
			};

			var claims = new List<Claim>
			{
				new Claim(ClaimTypes.NameIdentifier, user.Id.ToString())
			};

			var identity = new ClaimsIdentity(claims);
			var principal = new ClaimsPrincipal(identity);

			_controller.ControllerContext = new ControllerContext
			{
				HttpContext = new DefaultHttpContext
				{
					User = principal
				}
			};

			_mockUserManager
				.Setup(m => m.FindByIdAsync(user.Id.ToString()))
				.ReturnsAsync(user);

			// Act
			var result = await _controller.GetUserDetails();

			// Assert
			var okResult = Assert.IsType<OkObjectResult>(result);
			Assert.NotNull(okResult.Value);
		}
	}
}