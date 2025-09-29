# Day 8 – Add DTOs and AutoMapper

**Goal:** Decouple internal models from API responses using Data Transfer Objects (DTOs) and AutoMapper

**Estimated Time:** 45-60 minutes

## 📋 Step-by-Step Actions

### Step 1: Install AutoMapper Package
```bash
dotnet add package AutoMapper.Extensions.Microsoft.DependencyInjection
```

### Step 2: Create DTOs Folder Structure
Create the following folder structure:
```
DTOs/
├── Auth/
│   ├── RegisterRequest.cs
│   ├── LoginRequest.cs
│   └── AuthResponse.cs
└── Tasks/
    ├── TaskCreateDto.cs
    ├── TaskUpdateDto.cs
    └── TaskResponseDto.cs
```

### Step 3: Create Task DTOs

**Create `DTOs/Tasks/TaskCreateDto.cs`:**
```csharp
using System.ComponentModel.DataAnnotations;

namespace TaskTrackerApi.DTOs.Tasks;

public class TaskCreateDto
{
    [Required]
    [StringLength(200)]
    public string Title { get; set; } = string.Empty;
    
    [StringLength(1000)]
    public string? Description { get; set; }
    
    public DateTime? DueDate { get; set; }
}
```

**Create `DTOs/Tasks/TaskUpdateDto.cs`:**
```csharp
using System.ComponentModel.DataAnnotations;

namespace TaskTrackerApi.DTOs.Tasks;

public class TaskUpdateDto
{
    [Required]
    [StringLength(200)]
    public string Title { get; set; } = string.Empty;
    
    [StringLength(1000)]
    public string? Description { get; set; }
    
    public DateTime? DueDate { get; set; }
    
    public bool IsCompleted { get; set; }
}
```

**Create `DTOs/Tasks/TaskResponseDto.cs`:**
```csharp
namespace TaskTrackerApi.DTOs.Tasks;

public class TaskResponseDto
{
    public int Id { get; set; }
    public string Title { get; set; } = string.Empty;
    public string? Description { get; set; }
    public DateTime? DueDate { get; set; }
    public bool IsCompleted { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
}
```

### Step 4: Create Auth DTOs (if not already created)

**Create `DTOs/Auth/RegisterRequest.cs`:**
```csharp
using System.ComponentModel.DataAnnotations;

namespace TaskTrackerApi.DTOs.Auth;

public class RegisterRequest
{
    [Required]
    [StringLength(50, MinimumLength = 3)]
    public string Username { get; set; } = string.Empty;
    
    [Required]
    [StringLength(100, MinimumLength = 6)]
    public string Password { get; set; } = string.Empty;
}
```

**Create `DTOs/Auth/LoginRequest.cs`:**
```csharp
using System.ComponentModel.DataAnnotations;

namespace TaskTrackerApi.DTOs.Auth;

public class LoginRequest
{
    [Required]
    public string Username { get; set; } = string.Empty;
    
    [Required]
    public string Password { get; set; } = string.Empty;
}
```

**Create `DTOs/Auth/AuthResponse.cs`:**
```csharp
namespace TaskTrackerApi.DTOs.Auth;

public class AuthResponse
{
    public string Token { get; set; } = string.Empty;
    public string Username { get; set; } = string.Empty;
    public DateTime ExpiresAt { get; set; }
}
```

### Step 5: Create AutoMapper Profile

**Create `Mappings/AutoMapperProfile.cs`:**
```csharp
using AutoMapper;
using TaskTrackerApi.Models;
using TaskTrackerApi.DTOs.Tasks;
using TaskTrackerApi.DTOs.Auth;

namespace TaskTrackerApi.Mappings;

public class AutoMapperProfile : Profile
{
    public AutoMapperProfile()
    {
        // Task mappings
        CreateMap<TaskItem, TaskResponseDto>();
        CreateMap<TaskCreateDto, TaskItem>()
            .ForMember(dest => dest.CreatedAt, opt => opt.MapFrom(src => DateTime.UtcNow))
            .ForMember(dest => dest.UpdatedAt, opt => opt.MapFrom(src => DateTime.UtcNow));
        CreateMap<TaskUpdateDto, TaskItem>()
            .ForMember(dest => dest.UpdatedAt, opt => opt.MapFrom(src => DateTime.UtcNow));
        
        // User mappings
        CreateMap<User, AuthResponse>()
            .ForMember(dest => dest.Username, opt => opt.MapFrom(src => src.Username))
            .ForMember(dest => dest.Token, opt => opt.Ignore())
            .ForMember(dest => dest.ExpiresAt, opt => opt.Ignore());
    }
}
```

### Step 6: Configure AutoMapper in Program.cs

Add AutoMapper configuration to your `Program.cs`:
```csharp
// Add this line with other service registrations
builder.Services.AddAutoMapper(typeof(AutoMapperProfile));
```

### Step 7: Update TaskController to Use DTOs

Modify your `TaskController` to use DTOs instead of direct model binding:

**Key changes:**
- Inject `IMapper` into constructor
- Use DTOs for request/response parameters
- Use AutoMapper to convert between DTOs and models

**Example methods:**
```csharp
[HttpGet]
public async Task<ActionResult<IEnumerable<TaskResponseDto>>> GetTasks()
{
    var userId = GetUserIdFromClaims();
    var tasks = await _context.TaskItems
        .Where(t => t.UserId == userId)
        .ToListAsync();
    
    var taskDtos = _mapper.Map<List<TaskResponseDto>>(tasks);
    return Ok(taskDtos);
}

[HttpPost]
public async Task<ActionResult<TaskResponseDto>> CreateTask(TaskCreateDto taskDto)
{
    var userId = GetUserIdFromClaims();
    var taskItem = _mapper.Map<TaskItem>(taskDto);
    taskItem.UserId = userId;
    
    _context.TaskItems.Add(taskItem);
    await _context.SaveChangesAsync();
    
    var responseDto = _mapper.Map<TaskResponseDto>(taskItem);
    return CreatedAtAction(nameof(GetTask), new { id = taskItem.Id }, responseDto);
}
```

### Step 8: Update AuthController to Use DTOs

Modify your `AuthController` to use the new Auth DTOs:

**Example methods:**
```csharp
[HttpPost("register")]
public async Task<IActionResult> Register(RegisterRequest request)
{
    // ...existing validation logic...
    
    var user = new User
    {
        Username = request.Username,
        PasswordHash = hashedPassword
    };
    
    // ...existing save logic...
    
    return Ok(new { message = "User registered successfully" });
}

[HttpPost("login")]
public async Task<ActionResult<AuthResponse>> Login(LoginRequest request)
{
    // ...existing authentication logic...
    
    var response = _mapper.Map<AuthResponse>(user);
    response.Token = token;
    response.ExpiresAt = DateTime.UtcNow.AddHours(1);
    
    return Ok(response);
}
```

### Step 9: Test the Updated API

1. **Build and run the application:**
   ```bash
   dotnet build
   dotnet run
   ```

2. **Test with Swagger UI:**
   - Navigate to `/swagger`
   - Test registration with the new DTO structure
   - Test login and verify the response format
   - Test task CRUD operations with DTOs

3. **Verify DTO validation:**
   - Try creating a task with missing required fields
   - Try creating a task with strings that exceed length limits
   - Verify proper error responses

### Step 10: Verify AutoMapper Configuration

Add a test to ensure AutoMapper configuration is valid:

**Create `Tests/AutoMapperTests.cs` (optional):**
```csharp
using AutoMapper;
using TaskTrackerApi.Mappings;
using Xunit;

namespace TaskTrackerApi.Tests;

public class AutoMapperTests
{
    [Fact]
    public void AutoMapper_Configuration_IsValid()
    {
        var config = new MapperConfiguration(cfg => cfg.AddProfile<AutoMapperProfile>());
        config.AssertConfigurationIsValid();
    }
}
```

## ✅ Deliverable Checklist

- [ ] AutoMapper package installed
- [ ] DTOs created for all API requests/responses
- [ ] AutoMapper profile configured with proper mappings
- [ ] Controllers updated to use DTOs instead of direct model binding
- [ ] API still functions correctly with improved structure
- [ ] Swagger documentation reflects new DTO structure
- [ ] Input validation works through DTO data annotations

## 🎯 Success Criteria

**You've successfully completed Day 8 when:**
1. All API endpoints use DTOs for requests and responses
2. Internal models are no longer directly exposed through the API
3. AutoMapper handles conversions between DTOs and models
4. API responses have a clean, consistent structure
5. Input validation works seamlessly through DTO annotations

## 🔄 Next Steps

Tomorrow (Day 9), you'll focus on improving validation and error handling to make your API more robust and user-friendly.

## 💡 Key Benefits Achieved

- **Separation of Concerns:** API contracts are now independent of internal models
- **Version Flexibility:** Can modify internal models without breaking API contracts
- **Cleaner Code:** AutoMapper eliminates repetitive mapping code
- **Better Validation:** Centralized validation through DTO annotations
- **Improved Security:** Prevents over-posting by controlling which fields are exposed
