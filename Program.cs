using System.Security.Cryptography.Xml;
using System.Text;
using System.Text.Json.Serialization;
using Backend.Data;
using Backend.Models;
// using Backend.Service;
using FirebaseAdmin;
using Google.Apis.Auth.OAuth2;
using Google.Cloud.Firestore;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;

var builder = WebApplication.CreateBuilder(args);

// Load environment variables from .env file
DotNetEnv.Env.Load();

// var dbHost = Environment.GetEnvironmentVariable("DB_HOST");
// var dbPort = Environment.GetEnvironmentVariable("DB_PORT");
// var dbName = Environment.GetEnvironmentVariable("DB_NAME");
// var dbUser = Environment.GetEnvironmentVariable("DB_USER");
// var dbPass = Environment.GetEnvironmentVariable("DB_PASS");

// var connectionString = $"Host={dbHost}; Port={dbPort}; Database={dbName}; Username={dbUser}; Password={dbPass};";

// Add services to the container.
builder.Services.AddControllersWithViews();

builder.Services.AddControllers().AddJsonOptions(options =>
{
	options.JsonSerializerOptions.ReferenceHandler = ReferenceHandler.Preserve;
});

// Add DbContext and specify PostgreSQL connection
builder.Services.AddDbContext<ApplicationDbContext>(options =>
	options.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection"))
);

builder.Configuration.AddEnvironmentVariables();

// Add Identity services
builder
	.Services.AddIdentity<User, IdentityRole>(options =>
	{
		options.Password.RequireDigit = true;
		options.Password.RequireLowercase = true;
		options.Password.RequireNonAlphanumeric = false;
		options.Password.RequireUppercase = true;
		options.Password.RequiredLength = 8;
		options.Password.RequiredUniqueChars = 1;
	})
	.AddEntityFrameworkStores<ApplicationDbContext>()
	.AddDefaultTokenProviders();

// Configure JWT authentication
builder
	.Services.AddAuthentication(options =>
	{
		options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
		options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
		options.DefaultScheme = JwtBearerDefaults.AuthenticationScheme;
	})
	.AddJwtBearer(options =>
	{
		options.TokenValidationParameters =
			new Microsoft.IdentityModel.Tokens.TokenValidationParameters
			{
				ValidateIssuer = true,
				ValidateAudience = true,
				ValidateLifetime = true,
				ValidateIssuerSigningKey = true,
				ValidIssuer = builder.Configuration["Jwt:Issuer"],
				ValidAudience = builder.Configuration["Jwt:Audience"],
				IssuerSigningKey = new SymmetricSecurityKey(
					Encoding.UTF8.GetBytes(builder.Configuration["Jwt:Key"] ?? string.Empty)
				),
				ClockSkew = TimeSpan.Zero
			};
	});
builder.Services.AddAuthorization();

builder.Services.AddControllers();

// Add CORS policy
builder.Services.AddCors(options =>
{
	options.AddPolicy(
		"AllowAllOrigins",
		// "AllowAll",
		builder => builder.AllowAnyOrigin().AllowAnyMethod().AllowAnyHeader()
	);
});

// Firebase Admin SDK initialization
// Initialize and configure Firestore
// try
// {
//     // Load credentials from the specified file
//     var path = Path.Combine(Directory.GetCurrentDirectory(), "../Backend/task-mate-firebase-sdk.json");
//     if (!File.Exists(path))
//     {
//         throw new FileNotFoundException("Firebase service account file not found.", path);
//     }

//     var credential = GoogleCredential.FromFile(path);

//     // Initialize Firebase
//     var firebase = FirebaseApp.Create(
//         new AppOptions
//         {
//             Credential = credential,
//             ProjectId = builder.Configuration["Firebase:Web:projectId"]
//         }
//     );

//     Console.WriteLine("Firebase initialized successfully.");
//     Console.WriteLine(firebase);

//     // Additional Firebase operations (e.g., initializing Firestore)
//     builder.Services.AddSingleton<IFirestoreService>(provider =>
//     {
//         var projectId = builder.Configuration["Firebase:Web:projectId"];
//         return new FirestoreService(projectId ?? string.Empty);
//     });

//     Console.WriteLine("Firestore initialized successfully.");
// }
// catch (Exception ex)
// {
//     Console.WriteLine($"Firebase initialization failed: {ex.Message}");
// }

var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
	app.UseExceptionHandler("/Home/Error");
	// The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
	app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

app.UseCors("AllowAllOrigins");

app.UseAuthentication();
app.UseAuthorization();

app.MapControllerRoute(name: "default", pattern: "{controller=Home}/{action=Index}/{id?}");

// Ensure the database is created and migrate any pending migrations
using (var scope = app.Services.CreateScope())
{
	var services = scope.ServiceProvider;
	var dbContext = services.GetRequiredService<ApplicationDbContext>();
	dbContext.Database.Migrate();

	SeedData.Initialize(services);
}

app.Run();
