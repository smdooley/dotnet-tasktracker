# Day 6 – Protect Endpoints with Authorization

**Goal:** Restrict API access to authenticated users  
**Time Estimate:** ~45 minutes

## ✅ Tasks Overview
- Configure JWT authentication middleware in Program.cs
- Add authorization to API controllers
- Create a basic TaskController with protected endpoints
- Test the complete authentication flow

---

## 🔧 Step-by-Step Implementation

### Step 1: Configure JWT Authentication Middleware (10 mins)

The JWT authentication middleware is already configured in `Program.cs`. Verify it matches this structure:

```csharp
// JWT configuration should already be in Program.cs
var jwtSettings = builder.Configuration.GetSection("JwtSettings");
var secretKey = jwtSettings["SecretKey"];
var key = Encoding.UTF8.GetBytes(secretKey);

builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
})
.AddJwtBearer(options =>
{
    options.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuer = true,
        ValidateAudience = true,
        ValidateLifetime = true,
        ValidateIssuerSigningKey = true,
        ValidIssuer = jwtSettings["Issuer"],
        ValidAudience = jwtSettings["Audience"],
        IssuerSigningKey = new SymmetricSecurityKey(key)
    };
});
```

### Step 2: Update appsettings.json with JWT Configuration (5 mins)

Add JWT settings to `appsettings.json` (note the section name is "JwtSettings"):

```json
{
  "JwtSettings": {
    "SecretKey": "MyVerySecureSecretKeyThatIsAtLeast32CharactersLong!",
    "Issuer": "TaskTrackerApi",
    "Audience": "TaskTrackerApi",
    "ExpireMinutes": 60
  }
}
```

### Step 3: Create Protected TaskController (15 mins)

Create `Controllers/TaskController.cs`:

```csharp
[Route("api/[controller]")]
[ApiController]
[Authorize] // This protects all endpoints in this controller
public class TaskController : ControllerBase
{
    private readonly AppDbContext _context;

    public TaskController(AppDbContext context)
    {
        _context = context;
    }

    // GET: api/task
    [HttpGet]
    public async Task<ActionResult<IEnumerable<TaskItem>>> GetTasks()
    {
        var userId = GetCurrentUserId();
        var tasks = await _context.Tasks
            .Where(t => t.UserId == userId)
            .ToListAsync();
        
        return Ok(tasks);
    }

    // GET: api/task/{id}
    [HttpGet("{id}")]
    public async Task<ActionResult<TaskItem>> GetTask(int id)
    {
        var userId = GetCurrentUserId();
        var task = await _context.Tasks
            .FirstOrDefaultAsync(t => t.Id == id && t.UserId == userId);

        if (task == null)
        {
            return NotFound();
        }

        return Ok(task);
    }

    private int GetCurrentUserId()
    {
        var userIdClaim = User.FindFirst("userId")?.Value;
        return int.Parse(userIdClaim!);
    }
}
```

### Step 4: Verify Token Generation Includes UserId Claim (10 mins)

The `JwtService` is already configured to include the UserId claim. Verify your token generation includes:

```csharp
// In JwtService.GenerateToken method, these claims should already be present:
var tokenDescriptor = new SecurityTokenDescriptor
{
    Subject = new ClaimsIdentity(new[]
    {
        new Claim(JwtRegisteredClaimNames.Sub, user.Username),
        new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
        new Claim("userId", user.Id.ToString()) // This claim is needed for TaskController
    }),
    // ...existing code...
};
```

### Step 5: Test Authentication Flow (15 mins)

#### Test using Swagger UI (Recommended)
1. **Access Swagger:** Navigate to your API root (usually `https://localhost:7001`)
2. **Register a User:** Use `POST /api/auth/register` endpoint
3. **Login:** Use `POST /api/auth/login` to get a JWT token
4. **Authorize:** Click the "Authorize" button in Swagger UI and enter `Bearer [YOUR_TOKEN]`
5. **Test Protected Endpoints:** Try `GET /api/task` - it should now work

#### Alternative: Test with curl
```bash
# Test 1: Access Protected Endpoint Without Token (should return 401)
curl -X GET "https://localhost:7001/api/task" -H "accept: application/json"

# Test 2: Register a User
curl -X POST "https://localhost:7001/api/auth/register" \
  -H "Content-Type: application/json" \
  -d '{
    "username": "testuser",
    "password": "Test123!"
  }'

# Test 3: Login and Get Token
curl -X POST "https://localhost:7001/api/auth/login" \
  -H "Content-Type: application/json" \
  -d '{
    "username": "testuser",
    "password": "Test123!"
  }'

# Test 4: Access Protected Endpoint With Token
curl -X GET "https://localhost:7001/api/task" \
  -H "accept: application/json" \
  -H "Authorization: Bearer [YOUR_TOKEN]"
```

---

## 🎯 Expected Results

✅ **Unauthorized requests are blocked:** GET /api/task without token returns 401  
✅ **JWT-authenticated requests succeed:** GET /api/task with valid token returns 200  
✅ **User-specific data:** Tasks are filtered by the authenticated user's ID  
✅ **Swagger JWT support:** "Authorize" button appears in Swagger UI for easy testing

---

## 🐛 Common Issues & Solutions

### Issue: 401 Unauthorized even with valid token
**Solution:** Check that middleware order is correct: `UseAuthentication()` must come before `UseAuthorization()`

### Issue: "userId" claim not found
**Solution:** Ensure the token generation includes the userId claim and the claim name matches exactly

### Issue: JWT settings not found
**Solution:** Verify `appsettings.json` JWT configuration uses "JwtSettings" section name and contains "SecretKey", not "Key"

### Issue: Swagger Authorization button not working
**Solution:** Ensure the Bearer security scheme is properly configured in `AddSwaggerGen`

---

## ✅ Day 6 Checklist

- [x] JWT authentication middleware configured in Program.cs *(Already implemented)*
- [ ] JWT settings verified in appsettings.json with "JwtSettings" section
- [x] Swagger JWT authentication support enabled *(Already implemented)*
- [ ] TaskController created with [Authorize] attribute
- [x] UserId claim included in token generation *(Already implemented)*
- [ ] GetCurrentUserId() helper method implemented
- [ ] Tested: Unauthorized access returns 401
- [ ] Tested: Valid JWT token allows access to protected endpoints via Swagger UI
- [ ] Verified: Tasks are filtered by authenticated user

---

## 🚀 Ready for Day 7?

Tomorrow we'll implement full CRUD operations for tasks, allowing users to create, update, and delete their tasks through the protected API endpoints!
