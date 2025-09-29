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

**Actions:**
- Open your `RegisterRequest` DTO
- Add `[Required]`, `[StringLength]`, `[EmailAddress]` attributes
- Open your `TaskRequest` DTO  
- Add `[Required]`, `[StringLength]` attributes
- Add `[Range]` for numeric fields if applicable

**Example:**
```csharp
public class RegisterRequest
{
    [Required(ErrorMessage = "Username is required")]
    [StringLength(50, MinimumLength = 3, ErrorMessage = "Username must be between 3 and 50 characters")]
    public string Username { get; set; }

    [Required(ErrorMessage = "Password is required")]
    [StringLength(100, MinimumLength = 6, ErrorMessage = "Password must be at least 6 characters")]
    public string Password { get; set; }
}
```

### **Step 2: Apply [ApiController] Attribute (5 mins)**

**What:** Enable automatic model validation and standardized responses.

**Actions:**
- Add `[ApiController]` to all your controllers (`AuthController`, `TaskController`)
- Remove manual `ModelState.IsValid` checks (they're now automatic)
- Update route attributes to use `[Route("api/[controller]")]`

**Example:**
```csharp
[ApiController]
[Route("api/[controller]")]
public class AuthController : ControllerBase
{
    // ...existing methods...
}
```

### **Step 3: Create Standardized Error Response DTOs (10 mins)**

**What:** Create consistent error response format across the API.

**Actions:**
- Create `DTOs/ErrorResponse.cs`
- Create `DTOs/ValidationErrorResponse.cs`
- Define standard error structure with status code, message, and details

**Example:**
```csharp
public class ErrorResponse
{
    public int StatusCode { get; set; }
    public string Message { get; set; }
    public string Details { get; set; }
    public DateTime Timestamp { get; set; } = DateTime.UtcNow;
}
```

### **Step 4: Implement Proper HTTP Status Codes (10 mins)**

**What:** Return appropriate HTTP status codes for different scenarios.

**Actions:**
- Update controllers to return proper status codes:
  - `200 OK` for successful GET requests
  - `201 Created` for successful POST requests
  - `204 No Content` for successful DELETE requests
  - `400 Bad Request` for validation errors
  - `401 Unauthorized` for authentication failures
  - `403 Forbidden` for authorization failures
  - `404 Not Found` for missing resources
  - `409 Conflict` for duplicate resources

**Example:**
```csharp
[HttpPost]
public async Task<ActionResult<TaskResponse>> CreateTask(TaskRequest request)
{
    // ...create task logic...
    return CreatedAtAction(nameof(GetTask), new { id = task.Id }, taskResponse);
}

[HttpGet("{id}")]
public async Task<ActionResult<TaskResponse>> GetTask(int id)
{
    var task = await _context.Tasks.FindAsync(id);
    if (task == null)
        return NotFound(new ErrorResponse 
        { 
            StatusCode = 404, 
            Message = "Task not found" 
        });
    
    // ...return task...
}
```

### **Step 5: Add Custom Validation Attributes (10 mins)**

**What:** Create custom validation for business rules.

**Actions:**
- Create `Attributes/FutureDateAttribute.cs` for due dates
- Create `Attributes/NoScriptTagsAttribute.cs` to prevent XSS
- Apply to relevant DTO properties

**Example:**
```csharp
public class FutureDateAttribute : ValidationAttribute
{
    public override bool IsValid(object value)
    {
        if (value is DateTime date)
            return date > DateTime.Now;
        return true; // Allow null dates
    }
}
```

### **Step 6: Create Global Exception Handler Middleware (15 mins)**

**What:** Handle unhandled exceptions gracefully across the entire API.

**Actions:**
- Create `Middleware/ExceptionHandlingMiddleware.cs`
- Handle different exception types (ArgumentException, UnauthorizedAccessException, etc.)
- Log exceptions appropriately
- Return consistent error responses
- Register middleware in `Program.cs`

**Example middleware structure:**
```csharp
public class ExceptionHandlingMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<ExceptionHandlingMiddleware> _logger;

    public async Task InvokeAsync(HttpContext context)
    {
        try
        {
            await _next(context);
        }
        catch (Exception ex)
        {
            await HandleExceptionAsync(context, ex);
        }
    }
}
```

### **Step 7: Add Request/Response Logging (Optional - 5 mins)**

**What:** Log incoming requests and responses for debugging.

**Actions:**
- Enable request logging in `Program.cs`
- Add structured logging for key operations
- Log validation failures and authentication attempts

---

## 🧪 Testing Scenarios

Test these validation scenarios in Swagger or Postman:

1. **Registration with invalid data:**
   - Empty username/password
   - Username too short/long
   - Password too weak

2. **Task creation with invalid data:**
   - Empty title
   - Title too long
   - Invalid due date (past date)

3. **Authentication scenarios:**
   - Invalid credentials
   - Missing JWT token
   - Expired JWT token

4. **Resource not found:**
   - Access non-existent task
   - Delete already deleted task

5. **Authorization scenarios:**
   - Access another user's tasks
   - Modify tasks without permission

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
