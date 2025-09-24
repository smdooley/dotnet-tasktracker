# Day 4 – User Registration Endpoint

Here are the step-by-step actions for Day 4:

## Step 1: Create DTOs for User Registration

First, create a `DTOs` folder and add the registration request DTO:

```csharp
// filepath: /workspaces/dotnet-tasktracker/DTOs/RegisterRequest.cs
using System.ComponentModel.DataAnnotations;

namespace TaskTrackerApi.DTOs
{
    public class RegisterRequest
    {
        [Required]
        [StringLength(50, MinimumLength = 3)]
        public string Username { get; set; } = string.Empty;

        [Required]
        [StringLength(100, MinimumLength = 6)]
        public string Password { get; set; } = string.Empty;
    }
}
```

## Step 2: Create the AuthController

Create the `Controllers` folder if it doesn't exist and add the AuthController:

```csharp
// filepath: /workspaces/dotnet-tasktracker/Controllers/AuthController.cs
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TaskTrackerApi.Data;
using TaskTrackerApi.DTOs;
using TaskTrackerApi.Models;

namespace TaskTrackerApi.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AuthController : ControllerBase
    {
        private readonly AppDbContext _context;
        private readonly IPasswordHasher<User> _passwordHasher;

        public AuthController(AppDbContext context, IPasswordHasher<User> passwordHasher)
        {
            _context = context;
            _passwordHasher = passwordHasher;
        }

        /// <summary>
        /// Register a new user
        /// </summary>
        /// <param name="request">Registration details</param>
        /// <returns>Success message or error</returns>
        [HttpPost("register")]
        public async Task<IActionResult> Register(RegisterRequest request)
        {
            // Check if username already exists
            var existingUser = await _context.Users
                .FirstOrDefaultAsync(u => u.Username == request.Username);

            if (existingUser != null)
            {
                return BadRequest(new { message = "Username already exists" });
            }

            // Create new user
            var user = new User
            {
                Username = request.Username
            };

            // Hash the password
            user.PasswordHash = _passwordHasher.HashPassword(user, request.Password);

            // Save to database
            _context.Users.Add(user);
            await _context.SaveChangesAsync();

            return Ok(new { message = "User registered successfully", userId = user.Id });
        }
    }
}
```

## Step 3: Register Password Hasher Service

Update your `Program.cs` to register the password hasher service:

```csharp
// filepath: /workspaces/dotnet-tasktracker/Program.cs
// ...existing code...
builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseSqlite(builder.Configuration.GetConnectionString("DefaultConnection")));

// Add password hasher service
builder.Services.AddScoped<IPasswordHasher<User>, PasswordHasher<User>>();

// ...existing code...
```

## Step 4: Add Required NuGet Package

Install the Microsoft.AspNetCore.Identity package for password hashing:

```bash
dotnet add package Microsoft.AspNetCore.Identity.EntityFrameworkCore
```

## Step 5: Update User Model (if needed)

Ensure your User model has the necessary using statement:

```csharp
// filepath: /workspaces/dotnet-tasktracker/Models/User.cs
// ...existing code...
using System.ComponentModel.DataAnnotations;

namespace TaskTrackerApi.Models
{
    public class User
    {
        public int Id { get; set; }
        
        [Required]
        [StringLength(50)]
        public string Username { get; set; } = string.Empty;
        
        [Required]
        public string PasswordHash { get; set; } = string.Empty;
        
        // Navigation property for tasks
        public ICollection<TaskItem> Tasks { get; set; } = new List<TaskItem>();
    }
}
```

## Step 6: Test the Registration Endpoint

1. Build and run the application:
   ```bash
   dotnet build
   dotnet run
   ```

2. Open Swagger UI in your browser (typically at `https://localhost:7000/swagger` or similar)

3. Test the `POST /api/auth/register` endpoint with sample data:
   ```json
   {
     "username": "testuser",
     "password": "password123"
   }
   ```

## Step 7: Verify Database Storage

Check that the user was created in your SQLite database. You can use a SQLite browser or run a quick test query in your application to verify the user exists with a hashed password.

## Expected Deliverable

✅ **`POST /api/auth/register` creates user in DB**

Your registration endpoint should:
- Accept username and password
- Validate input (required fields, length constraints)
- Check for duplicate usernames
- Hash the password securely
- Save the user to the SQLite database
- Return appropriate success/error responses

The password will be securely hashed using ASP.NET Core Identity's built-in password hasher, and you should see the new user record in your database with the hashed password stored safely.