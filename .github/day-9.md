# Day 9 – Improve Validation & Error Handling

**Goal:** Add robust input validation and error responses to make the API more reliable and user-friendly.

**Time Estimate:** ~45-60 minutes

---

## 📋 Tasks Overview

1. Add data annotations for model validation
2. Implement `[ApiController]` attribute for automatic validation
3. Create standardized error response DTOs
4. Add custom validation attributes
5. Implement proper HTTP status codes
6. Add global exception handling middleware
7. Test validation scenarios

---

## 🛠 Step-by-Step Implementation

### **Step 1: Add Data Annotations to DTOs (10 mins)**

**What:** Add validation attributes to your request DTOs to ensure data integrity.

**Update RegisterRequest DTO:**
```csharp
// DTOs/RegisterRequest.cs
using System.ComponentModel.DataAnnotations;

public class RegisterRequest
{
    [Required(ErrorMessage = "Username is required")]
    [StringLength(50, MinimumLength = 3, ErrorMessage = "Username must be between 3 and 50 characters")]
    public string Username { get; set; } = string.Empty;

    [Required(ErrorMessage = "Password is required")]
    [StringLength(100, MinimumLength = 6, ErrorMessage = "Password must be at least 6 characters")]
    public string Password { get; set; } = string.Empty;
}
```

**Update TaskRequest DTO:**
```csharp
// DTOs/TaskRequest.cs
using System.ComponentModel.DataAnnotations;

public class TaskRequest
{
    [Required(ErrorMessage = "Title is required")]
    [StringLength(200, MinimumLength = 1, ErrorMessage = "Title must be between 1 and 200 characters")]
    public string Title { get; set; } = string.Empty;

    [StringLength(1000, ErrorMessage = "Description cannot exceed 1000 characters")]
    public string? Description { get; set; }

    [DataType(DataType.DateTime)]
    public DateTime? DueDate { get; set; }

    public bool IsCompleted { get; set; } = false;
}
```

### **Step 2: Apply [ApiController] Attribute (5 mins)**

**Update AuthController:**
```csharp
// Controllers/AuthController.cs
[ApiController]
[Route("api/[controller]")]
public class AuthController : ControllerBase
{
    // ...existing code...
    
    [HttpPost("register")]
    public async Task<ActionResult<UserResponse>> Register(RegisterRequest request)
    {
        // Remove ModelState.IsValid check - handled automatically now
        
        // ...existing registration logic...
    }

    [HttpPost("login")]
    public async Task<ActionResult<LoginResponse>> Login(LoginRequest request)
    {
        // Remove ModelState.IsValid check - handled automatically now
        
        // ...existing login logic...
    }
}
```

**Update TaskController:**
```csharp
// Controllers/TaskController.cs
[ApiController]
[Route("api/[controller]")]
[Authorize]
public class TaskController : ControllerBase
{
    // ...existing code...
}
```

### **Step 3: Create Standardized Error Response DTOs (10 mins)**

**Create ErrorResponse DTO:**
```csharp
// DTOs/ErrorResponse.cs
namespace TaskTrackerApi.DTOs
{
    public class ErrorResponse
    {
        public int StatusCode { get; set; }
        public string Message { get; set; } = string.Empty;
        public string? Details { get; set; }
        public DateTime Timestamp { get; set; } = DateTime.UtcNow;
    }
}
```

**Create ValidationErrorResponse DTO:**
```csharp
// DTOs/ValidationErrorResponse.cs
namespace TaskTrackerApi.DTOs
{
    public class ValidationErrorResponse : ErrorResponse
    {
        public Dictionary<string, string[]> Errors { get; set; } = new();
    }
}
```

### **Step 4: Implement Proper HTTP Status Codes (10 mins)**

**Update TaskController methods:**
```csharp
// Controllers/TaskController.cs
public class TaskController : ControllerBase
{
    // ...existing code...

    [HttpPost]
    public async Task<ActionResult<TaskResponse>> CreateTask(TaskRequest request)
    {
        var userId = GetCurrentUserId();
        
        var task = new TaskItem
        {
            Title = request.Title,
            Description = request.Description,
            DueDate = request.DueDate,
            IsCompleted = request.IsCompleted,
            UserId = userId,
            CreatedAt = DateTime.UtcNow
        };

        _context.Tasks.Add(task);
        await _context.SaveChangesAsync();

        var response = _mapper.Map<TaskResponse>(task);
        return CreatedAtAction(nameof(GetTask), new { id = task.Id }, response);
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<TaskResponse>> GetTask(int id)
    {
        var userId = GetCurrentUserId();
        var task = await _context.Tasks
            .Where(t => t.UserId == userId && t.Id == id)
            .FirstOrDefaultAsync();

        if (task == null)
        {
            return NotFound(new ErrorResponse 
            { 
                StatusCode = 404, 
                Message = "Task not found",
                Details = $"No task found with ID {id} for the current user"
            });
        }

        return Ok(_mapper.Map<TaskResponse>(task));
    }

    [HttpPut("{id}")]
    public async Task<IActionResult> UpdateTask(int id, TaskRequest request)
    {
        var userId = GetCurrentUserId();
        var task = await _context.Tasks
            .Where(t => t.UserId == userId && t.Id == id)
            .FirstOrDefaultAsync();

        if (task == null)
        {
            return NotFound(new ErrorResponse 
            { 
                StatusCode = 404, 
                Message = "Task not found" 
            });
        }

        task.Title = request.Title;
        task.Description = request.Description;
        task.DueDate = request.DueDate;
        task.IsCompleted = request.IsCompleted;
        task.UpdatedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync();
        return NoContent();
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> DeleteTask(int id)
    {
        var userId = GetCurrentUserId();
        var task = await _context.Tasks
            .Where(t => t.UserId == userId && t.Id == id)
            .FirstOrDefaultAsync();

        if (task == null)
        {
            return NotFound(new ErrorResponse 
            { 
                StatusCode = 404, 
                Message = "Task not found" 
            });
        }

        _context.Tasks.Remove(task);
        await _context.SaveChangesAsync();
        return NoContent();
    }
}
```

### **Step 5: Add Custom Validation Attributes (10 mins)**

**Create FutureDateAttribute:**
```csharp
// Attributes/FutureDateAttribute.cs
using System.ComponentModel.DataAnnotations;

namespace TaskTrackerApi.Attributes
{
    public class FutureDateAttribute : ValidationAttribute
    {
        protected override ValidationResult? IsValid(object? value, ValidationContext validationContext)
        {
            if (value == null) return ValidationResult.Success; // Allow null dates

            if (value is DateTime date)
            {
                if (date <= DateTime.Now)
                {
                    return new ValidationResult("Due date must be in the future");
                }
            }

            return ValidationResult.Success;
        }
    }
}
```

**Create NoScriptTagsAttribute:**
```csharp
// Attributes/NoScriptTagsAttribute.cs
using System.ComponentModel.DataAnnotations;
using System.Text.RegularExpressions;

namespace TaskTrackerApi.Attributes
{
    public class NoScriptTagsAttribute : ValidationAttribute
    {
        protected override ValidationResult? IsValid(object? value, ValidationContext validationContext)
        {
            if (value == null) return ValidationResult.Success;

            var input = value.ToString();
            if (string.IsNullOrEmpty(input)) return ValidationResult.Success;

            // Check for script tags or javascript: protocol
            var scriptPattern = @"<script\b[^<]*(?:(?!<\/script>)<[^<]*)*<\/script>";
            var jsPattern = @"javascript:";

            if (Regex.IsMatch(input, scriptPattern, RegexOptions.IgnoreCase) ||
                Regex.IsMatch(input, jsPattern, RegexOptions.IgnoreCase))
            {
                return new ValidationResult("Input contains potentially dangerous content");
            }

            return ValidationResult.Success;
        }
    }
}
```

**Update TaskRequest with custom validation:**
```csharp
// DTOs/TaskRequest.cs
using TaskTrackerApi.Attributes;

public class TaskRequest
{
    [Required(ErrorMessage = "Title is required")]
    [StringLength(200, MinimumLength = 1, ErrorMessage = "Title must be between 1 and 200 characters")]
    [NoScriptTags]
    public string Title { get; set; } = string.Empty;

    [StringLength(1000, ErrorMessage = "Description cannot exceed 1000 characters")]
    [NoScriptTags]
    public string? Description { get; set; }

    [FutureDate]
    public DateTime? DueDate { get; set; }

    public bool IsCompleted { get; set; } = false;
}
```

### **Step 6: Create Global Exception Handler Middleware (15 mins)**

**Create ExceptionHandlingMiddleware:**
```csharp
// Middleware/ExceptionHandlingMiddleware.cs
using System.Net;
using System.Text.Json;
using TaskTrackerApi.DTOs;

namespace TaskTrackerApi.Middleware
{
    public class ExceptionHandlingMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly ILogger<ExceptionHandlingMiddleware> _logger;

        public ExceptionHandlingMiddleware(RequestDelegate next, ILogger<ExceptionHandlingMiddleware> logger)
        {
            _next = next;
            _logger = logger;
        }

        public async Task InvokeAsync(HttpContext context)
        {
            try
            {
                await _next(context);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An unhandled exception occurred");
                await HandleExceptionAsync(context, ex);
            }
        }

        private static async Task HandleExceptionAsync(HttpContext context, Exception exception)
        {
            context.Response.ContentType = "application/json";

            var errorResponse = exception switch
            {
                ArgumentException => new ErrorResponse
                {
                    StatusCode = (int)HttpStatusCode.BadRequest,
                    Message = "Invalid argument provided",
                    Details = exception.Message
                },
                UnauthorizedAccessException => new ErrorResponse
                {
                    StatusCode = (int)HttpStatusCode.Unauthorized,
                    Message = "Unauthorized access",
                    Details = exception.Message
                },
                KeyNotFoundException => new ErrorResponse
                {
                    StatusCode = (int)HttpStatusCode.NotFound,
                    Message = "Resource not found",
                    Details = exception.Message
                },
                _ => new ErrorResponse
                {
                    StatusCode = (int)HttpStatusCode.InternalServerError,
                    Message = "An internal server error occurred",
                    Details = "Please contact support if the problem persists"
                }
            };

            context.Response.StatusCode = errorResponse.StatusCode;

            var jsonResponse = JsonSerializer.Serialize(errorResponse, new JsonSerializerOptions
            {
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase
            });

            await context.Response.WriteAsync(jsonResponse);
        }
    }
}
```

**Register middleware in Program.cs:**
```csharp
// Program.cs
using TaskTrackerApi.Middleware;

var builder = WebApplication.CreateBuilder(args);

// ...existing code...

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

// Add exception handling middleware FIRST
app.UseMiddleware<ExceptionHandlingMiddleware>();

app.UseHttpsRedirection();
app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();

app.Run();
```

### **Step 7: Add Request/Response Logging (Optional - 5 mins)**

**Add logging to Program.cs:**
```csharp
// Program.cs
var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllers();
// ...existing services...

// Add request logging
builder.Services.AddHttpLogging(logging =>
{
    logging.LoggingFields = Microsoft.AspNetCore.HttpLogging.HttpLoggingFields.RequestPath |
                           Microsoft.AspNetCore.HttpLogging.HttpLoggingFields.RequestMethod |
                           Microsoft.AspNetCore.HttpLogging.HttpLoggingFields.ResponseStatusCode;
});

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
    app.UseHttpLogging(); // Enable request logging in development
}

// ...rest of middleware...
```

---

## 🧪 Testing Scenarios

Test these validation scenarios in Swagger or Postman:

1. **Registration with invalid data:**
   ```json
   // POST /api/auth/register
   {
     "username": "ab", // Too short
     "password": "123"  // Too short
   }
   ```

2. **Task creation with invalid data:**
   ```json
   // POST /api/task
   {
     "title": "",  // Empty title
     "description": "A".repeat(1001), // Too long
     "dueDate": "2020-01-01T00:00:00Z" // Past date
   }
   ```

3. **XSS prevention test:**
   ```json
   // POST /api/task
   {
     "title": "<script>alert('xss')</script>Malicious Task",
     "description": "javascript:void(0)"
   }
   ```

---

## 📝 Expected Outputs

After completing Day 9, your API should:

✅ **Validate input automatically** with clear error messages  
✅ **Return appropriate HTTP status codes** for all scenarios  
✅ **Handle exceptions gracefully** without exposing internal details  
✅ **Provide consistent error response format** across all endpoints  
✅ **Log important events** for debugging and monitoring  
✅ **Prevent common security issues** like XSS through validation  

---

## 🎯 Success Criteria

- [ ] All DTOs have proper validation attributes
- [ ] Controllers use `[ApiController]` attribute
- [ ] Standardized error responses implemented
- [ ] Custom validation attributes created and applied
- [ ] Global exception handler middleware implemented
- [ ] All HTTP status codes are appropriate
- [ ] Validation scenarios tested successfully
- [ ] No unhandled exceptions reach the client

---

## 🚀 Next Steps

Tomorrow (Day 10), you'll add final polish including Swagger JWT authentication support and complete end-to-end testing!

**Files you'll likely modify today:**
- `DTOs/` - Add validation attributes
- `Controllers/` - Update error handling
- `Middleware/` - Create exception handler
- `Program.cs` - Register middleware
- `Attributes/` - Custom validation (new folder)
