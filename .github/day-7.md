# 🗓 Day 7 – Implement CRUD for Task Items

**Goal:** Let users manage their tasks with full CRUD operations

**Time Estimate:** ~45-60 minutes

---

## 📋 Tasks Overview

By the end of Day 7, you'll have:
- A complete `TaskController` with all CRUD operations
- JWT-based user isolation (users only see their own tasks)
- Proper HTTP status codes and RESTful responses
- Full integration with your existing authentication system

---

## 🚀 Step-by-Step Instructions

### **Step 1: Create the TaskController** (10 mins)

Create a new controller file:

**File:** `Controllers/TaskController.cs`

```csharp
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;
using TaskTrackerApi.Data;
using TaskTrackerApi.Models;

namespace TaskTrackerApi.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize] // Protect all endpoints
    public class TaskController : ControllerBase
    {
        private readonly AppDbContext _context;

        public TaskController(AppDbContext context)
        {
            _context = context;
        }

        // Helper method to get current user ID from JWT
        private int GetCurrentUserId()
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            return int.Parse(userIdClaim);
        }
    }
}
```

### **Step 2: Implement GET /tasks (Get All Tasks)** (8 mins)

Add the method to retrieve all tasks for the current user:

```csharp
/// <summary>
/// Get all tasks for the authenticated user
/// </summary>
[HttpGet]
public async Task<ActionResult<IEnumerable<TaskItem>>> GetTasks()
{
    var userId = GetCurrentUserId();
    
    var tasks = await _context.Tasks
        .Where(t => t.UserId == userId)
        .OrderByDescending(t => t.CreatedAt)
        .ToListAsync();
    
    return Ok(tasks);
}
```

### **Step 3: Implement GET /tasks/{id} (Get Single Task)** (8 mins)

Add the method to retrieve a specific task:

```csharp
/// <summary>
/// Get a specific task by ID (only if owned by current user)
/// </summary>
[HttpGet("{id}")]
public async Task<ActionResult<TaskItem>> GetTask(int id)
{
    var userId = GetCurrentUserId();
    
    var task = await _context.Tasks
        .FirstOrDefaultAsync(t => t.Id == id && t.UserId == userId);
    
    if (task == null)
    {
        return NotFound($"Task with ID {id} not found or you don't have permission to access it.");
    }
    
    return Ok(task);
}
```

### **Step 4: Implement POST /tasks (Create Task)** (10 mins)

Add the method to create a new task:

```csharp
/// <summary>
/// Create a new task
/// </summary>
[HttpPost]
public async Task<ActionResult<TaskItem>> CreateTask(CreateTaskDto createTaskDto)
{
    var userId = GetCurrentUserId();
    
    var task = new TaskItem
    {
        Title = createTaskDto.Title,
        Description = createTaskDto.Description,
        DueDate = createTaskDto.DueDate,
        IsCompleted = false,
        UserId = userId,
        CreatedAt = DateTime.UtcNow,
        UpdatedAt = DateTime.UtcNow
    };
    
    _context.Tasks.Add(task);
    await _context.SaveChangesAsync();
    
    return CreatedAtAction(nameof(GetTask), new { id = task.Id }, task);
}
```

**Create the DTO:** `DTOs/CreateTaskDto.cs`

```csharp
using System.ComponentModel.DataAnnotations;

namespace TaskTrackerApi.DTOs
{
    public class CreateTaskDto
    {
        [Required]
        [StringLength(200)]
        public string Title { get; set; } = string.Empty;
        
        [StringLength(1000)]
        public string? Description { get; set; }
        
        public DateTime? DueDate { get; set; }
    }
}
```

### **Step 5: Implement PUT /tasks/{id} (Update Task)** (10 mins)

Add the method to update an existing task:

```csharp
/// <summary>
/// Update an existing task
/// </summary>
[HttpPut("{id}")]
public async Task<IActionResult> UpdateTask(int id, UpdateTaskDto updateTaskDto)
{
    var userId = GetCurrentUserId();
    
    var task = await _context.Tasks
        .FirstOrDefaultAsync(t => t.Id == id && t.UserId == userId);
    
    if (task == null)
    {
        return NotFound($"Task with ID {id} not found or you don't have permission to update it.");
    }
    
    // Update properties
    task.Title = updateTaskDto.Title;
    task.Description = updateTaskDto.Description;
    task.DueDate = updateTaskDto.DueDate;
    task.IsCompleted = updateTaskDto.IsCompleted;
    task.UpdatedAt = DateTime.UtcNow;
    
    await _context.SaveChangesAsync();
    
    return NoContent(); // 204 No Content for successful update
}
```

**Create the DTO:** `DTOs/UpdateTaskDto.cs`

```csharp
using System.ComponentModel.DataAnnotations;

namespace TaskTrackerApi.DTOs
{
    public class UpdateTaskDto
    {
        [Required]
        [StringLength(200)]
        public string Title { get; set; } = string.Empty;
        
        [StringLength(1000)]
        public string? Description { get; set; }
        
        public DateTime? DueDate { get; set; }
        
        public bool IsCompleted { get; set; }
    }
}
```

### **Step 6: Implement DELETE /tasks/{id} (Delete Task)** (8 mins)

Add the method to delete a task:

```csharp
/// <summary>
/// Delete a task
/// </summary>
[HttpDelete("{id}")]
public async Task<IActionResult> DeleteTask(int id)
{
    var userId = GetCurrentUserId();
    
    var task = await _context.Tasks
        .FirstOrDefaultAsync(t => t.Id == id && t.UserId == userId);
    
    if (task == null)
    {
        return NotFound($"Task with ID {id} not found or you don't have permission to delete it.");
    }
    
    _context.Tasks.Remove(task);
    await _context.SaveChangesAsync();
    
    return NoContent(); // 204 No Content for successful deletion
}
```

---

## 🧪 Testing Your CRUD Operations

### **Step 7: Test the Complete Flow** (10 mins)

1. **Start your API:**
   ```bash
   dotnet run
   ```

2. **Register and Login** (get your JWT token):
   ```bash
   # Register
   curl -X POST https://localhost:7097/api/auth/register \
     -H "Content-Type: application/json" \
     -d '{"username":"testuser","password":"TestPass123!"}'
   
   # Login (save the token)
   curl -X POST https://localhost:7097/api/auth/login \
     -H "Content-Type: application/json" \
     -d '{"username":"testuser","password":"TestPass123!"}'
   ```

3. **Test CRUD Operations** (replace `YOUR_JWT_TOKEN` with actual token):
   ```bash
   # Create a task
   curl -X POST https://localhost:7097/api/task \
     -H "Authorization: Bearer YOUR_JWT_TOKEN" \
     -H "Content-Type: application/json" \
     -d '{"title":"Learn .NET","description":"Complete Day 7 challenge","dueDate":"2024-01-15T10:00:00"}'
   
   # Get all tasks
   curl -X GET https://localhost:7097/api/task \
     -H "Authorization: Bearer YOUR_JWT_TOKEN"
   
   # Get specific task (replace {id} with actual ID)
   curl -X GET https://localhost:7097/api/task/{id} \
     -H "Authorization: Bearer YOUR_JWT_TOKEN"
   
   # Update task
   curl -X PUT https://localhost:7097/api/task/{id} \
     -H "Authorization: Bearer YOUR_JWT_TOKEN" \
     -H "Content-Type: application/json" \
     -d '{"title":"Learn .NET - Updated","description":"Complete Day 7 challenge","isCompleted":true,"dueDate":"2024-01-15T10:00:00"}'
   
   # Delete task
   curl -X DELETE https://localhost:7097/api/task/{id} \
     -H "Authorization: Bearer YOUR_JWT_TOKEN"
   ```

### **Alternative: Use Swagger UI**

1. Navigate to `https://localhost:7097/swagger`
2. Click "Authorize" and enter your JWT token as: `Bearer YOUR_JWT_TOKEN`
3. Test all endpoints through the Swagger interface

---

## ✅ Success Criteria

By the end of Day 7, you should have:

- [ ] ✅ **TaskController** with all 5 CRUD endpoints
- [ ] ✅ **User Isolation** - users only see/modify their own tasks
- [ ] ✅ **Proper HTTP Status Codes** (200, 201, 204, 404, etc.)
- [ ] ✅ **DTOs** for create/update operations
- [ ] ✅ **Authorization** on all endpoints
- [ ] ✅ **Tested** all operations via Swagger or curl

---

## 🐛 Common Issues & Solutions

**Issue:** `ClaimTypes.NameIdentifier` returns null
**Solution:** Ensure your JWT token includes the user ID claim in the login method

**Issue:** Tasks not filtering by user
**Solution:** Double-check the `GetCurrentUserId()` helper method and ensure `UserId` is set on task creation

**Issue:** 401 Unauthorized on all requests
**Solution:** Verify JWT token format: `Bearer <token>` (note the space)

---

## 🎯 Next Steps

Tomorrow (Day 8), you'll:
- Add DTOs for responses (not just requests)
- Implement AutoMapper for cleaner object mapping
- Separate concerns further with a service layer

Great job completing Day 7! You now have a fully functional, secure task management API! 🎉
