# Day 5 – JWT Authentication Setup

Here's a step-by-step guide to implement JWT authentication for your TaskTracker API:

## Step 1: Add JWT NuGet Package

Run this command in your terminal:

```bash
dotnet add package Microsoft.AspNetCore.Authentication.JwtBearer
```

## Step 2: Configure JWT Settings in appsettings.json

Add JWT configuration to your `appsettings.json`:

```json
{
  "Logging": {
    "LogLevel": {
      "Default": "Information",
      "Microsoft.AspNetCore": "Warning"
    }
  },
  "AllowedHosts": "*",
  "ConnectionStrings": {
    "DefaultConnection": "Data Source=tasktracker.db"
  },
  "JwtSettings": {
    "SecretKey": "YourSuperSecretKeyThatIsAtLeast32CharactersLong!",
    "Issuer": "TaskTrackerApi",
    "Audience": "TaskTrackerApi",
    "ExpiryInMinutes": 60
  }
}
```

## Step 3: Create JWT Service

Create a service to handle JWT token generation:

```csharp
// Services/IJwtService.cs
using TaskTrackerApi.Models;

namespace TaskTrackerApi.Services
{
    public interface IJwtService
    {
        string GenerateToken(User user);
    }
}
```

```csharp
// Services/JwtService.cs
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using TaskTrackerApi.Models;

namespace TaskTrackerApi.Services
{
    public class JwtService : IJwtService
    {
        private readonly IConfiguration _configuration;

        public JwtService(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        public string GenerateToken(User user)
        {
            var jwtSettings = _configuration.GetSection("JwtSettings");
            var secretKey = jwtSettings["SecretKey"];
            var key = Encoding.ASCII.GetBytes(secretKey);

            var tokenDescriptor = new SecurityTokenDescriptor
            {
                Subject = new ClaimsIdentity(new[]
                {
                    new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
                    new Claim(ClaimTypes.Name, user.Username)
                }),
                Expires = DateTime.UtcNow.AddMinutes(Convert.ToDouble(jwtSettings["ExpiryInMinutes"])),
                Issuer = jwtSettings["Issuer"],
                Audience = jwtSettings["Audience"],
                SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(key), SecurityAlgorithms.HmacSha256Signature)
            };

            var tokenHandler = new JwtSecurityTokenHandler();
            var token = tokenHandler.CreateToken(tokenDescriptor);
            return tokenHandler.WriteToken(token);
        }
    }
}
```

## Step 4: Configure JWT Authentication in Program.cs

Update your `Program.cs` to configure JWT authentication:

```csharp
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using System.Text;
using TaskTrackerApi.Data;
using TaskTrackerApi.Services;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseSqlite(builder.Configuration.GetConnectionString("DefaultConnection")));

// Add JWT Service
builder.Services.AddScoped<IJwtService, JwtService>();

// Configure JWT Authentication
var jwtSettings = builder.Configuration.GetSection("JwtSettings");
var secretKey = jwtSettings["SecretKey"];
var key = Encoding.ASCII.GetBytes(secretKey);

builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
})
.AddJwtBearer(options =>
{
    options.RequireHttpsMetadata = false;
    options.SaveToken = true;
    options.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuerSigningKey = true,
        IssuerSigningKey = new SymmetricSecurityKey(key),
        ValidateIssuer = true,
        ValidIssuer = jwtSettings["Issuer"],
        ValidateAudience = true,
        ValidAudience = jwtSettings["Audience"],
        ValidateLifetime = true,
        ClockSkew = TimeSpan.Zero
    };
});

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

// Add Authentication middleware (must be before Authorization)
app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

app.Run();
```

## Step 5: Create Login DTO

Create a DTO for login requests:

```csharp
// DTOs/LoginRequest.cs
using System.ComponentModel.DataAnnotations;

namespace TaskTrackerApi.DTOs
{
    public class LoginRequest
    {
        [Required]
        public string Username { get; set; } = string.Empty;

        [Required]
        public string Password { get; set; } = string.Empty;
    }
}
```

```csharp
// DTOs/LoginResponse.cs
namespace TaskTrackerApi.DTOs
{
    public class LoginResponse
    {
        public string Token { get; set; } = string.Empty;
        public string Username { get; set; } = string.Empty;
        public DateTime ExpiresAt { get; set; }
    }
}
```

## Step 6: Implement Login Endpoint in AuthController

Update your existing `AuthController` to include the login endpoint:

```csharp
// Controllers/AuthController.cs
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TaskTrackerApi.Data;
using TaskTrackerApi.DTOs;
using TaskTrackerApi.Models;
using TaskTrackerApi.Services;

namespace TaskTrackerApi.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AuthController : ControllerBase
    {
        private readonly AppDbContext _context;
        private readonly PasswordHasher<User> _passwordHasher;
        private readonly IJwtService _jwtService;
        private readonly IConfiguration _configuration;

        public AuthController(
            AppDbContext context, 
            IPasswordHasher<User> passwordHasher,
            IJwtService jwtService, 
            IConfiguration configuration)
        {
            _context = context;
            _passwordHasher = passwordHasher;
            _jwtService = jwtService;
            _configuration = configuration;
        }

        // ...existing code... (register endpoint from Day 4)

        /// <summary>
        /// Authenticate user and return JWT token
        /// </summary>
        [HttpPost("login")]
        public async Task<ActionResult<LoginResponse>> Login([FromBody] LoginRequest request)
        {
            // Find user by username
            var user = await _context.Users
                .FirstOrDefaultAsync(u => u.Username == request.Username);

            if (user == null)
            {
                return Unauthorized("Invalid username or password");
            }

            // Verify password
            var passwordResult = _passwordHasher.VerifyHashedPassword(
                user, user.PasswordHash, request.Password);

            if (passwordResult == PasswordVerificationResult.Failed)
            {
                return Unauthorized("Invalid username or password");
            }

            // Generate JWT token
            var token = _jwtService.GenerateToken(user);
            var expiryInMinutes = Convert.ToDouble(_configuration["JwtSettings:ExpiryInMinutes"]);

            var response = new LoginResponse
            {
                Token = token,
                Username = user.Username,
                ExpiresAt = DateTime.UtcNow.AddMinutes(expiryInMinutes)
            };

            return Ok(response);
        }
    }
}
```

## Step 7: Test the Implementation

1. **Build and run your application:**
   ```bash
   dotnet run
   ```

2. **Test the login endpoint** using Swagger or a tool like Postman:
   - **Endpoint:** `POST /api/auth/login`
   - **Body:**
     ```json
     {
       "username": "testuser",
       "password": "testpassword"
     }
     ```

3. **Expected response:**
   ```json
   {
     "token": "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9...",
     "username": "testuser",
     "expiresAt": "2025-09-24T15:30:00Z"
   }
   ```

## Verification Steps

- ✅ JWT package installed successfully
- ✅ JWT configuration added to appsettings.json
- ✅ JWT service created and registered
- ✅ Authentication middleware configured in Program.cs
- ✅ Login endpoint returns valid JWT token
- ✅ Token contains user claims (NameIdentifier, Name)

**Deliverable:** `POST /api/auth/login` returns a valid JWT token that you can decode and verify contains the correct user information.