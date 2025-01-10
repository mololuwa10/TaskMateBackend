using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using _.Models;
using Npgsql;
using Microsoft.Extensions.Configuration;

namespace _.Controllers;

public class HomeController(ILogger<HomeController> logger) : Controller
{
	private readonly ILogger<HomeController> _logger = logger;
	
	private readonly IConfiguration? _configuration;

    public IActionResult Index()
	{
		_logger.LogInformation("Checking database connection....");

		if (!CheckDatabaseConnection())
		{
			return StatusCode(500, "Failed to connect to the database");
		}

		return Ok("Database connection successful");
	}


	public IActionResult Privacy()
	{
		return View();
	}

	[ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
	public IActionResult Error()
	{
		return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
	}
	
	public bool CheckDatabaseConnection()
	{
		try
		{
			using (var connection = new NpgsqlConnection(_configuration?.GetConnectionString("ConnectionStrings:DefaultConnection")))
			{
				connection.Open(); // Attempt to open a connection
				return true; // Connection successful
			}
		}
		catch (Exception ex)
		{
			_logger.LogError("Database connection failed: {Message}", ex.Message);
			return false;
		}
	}

}
