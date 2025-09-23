# Day 3 – Set Up EF Core DbContext

Here are the step-by-step actions for Day 3:

## Step 1: Create the AppDbContext Class

Create a new folder called `Data` in your project root and add the `AppDbContext.cs` file:

```csharp
// filepath: /workspaces/dotnet-tasktracker/Data/AppDbContext.cs
using Microsoft.EntityFrameworkCore;
using TaskTrackerApi.Models;

namespace TaskTrackerApi.Data
{
    /// <summary>
    /// Database context for the TaskTracker application
    /// </summary>
    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
        {
        }

        /// <summary>
        /// Users table
        /// </summary>
        public DbSet<User> Users { get; set; }

        /// <summary>
        /// Tasks table
        /// </summary>
        public DbSet<TaskItem> Tasks { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // Configure User entity
            modelBuilder.Entity<User>(entity =>
            {
                entity.HasKey(u => u.Id);
                entity.Property(u => u.Username).IsRequired().HasMaxLength(50);
                entity.Property(u => u.PasswordHash).IsRequired();
                entity.HasIndex(u => u.Username).IsUnique();
            });

            // Configure TaskItem entity
            modelBuilder.Entity<TaskItem>(entity =>
            {
                entity.HasKey(t => t.Id);
                entity.Property(t => t.Title).IsRequired().HasMaxLength(200);
                entity.Property(t => t.Description).HasMaxLength(1000);
                entity.Property(t => t.CreatedAt).IsRequired();
                entity.Property(t => t.UpdatedAt).IsRequired();

                // Configure relationship
                entity.HasOne(t => t.User)
                      .WithMany(u => u.Tasks)
                      .HasForeignKey(t => t.UserId)
                      .OnDelete(DeleteBehavior.Cascade);
            });
        }
    }
}
```

## Step 2: Update Program.cs to Register DbContext

Add the DbContext configuration to your `Program.cs` file:

```csharp
// filepath: /workspaces/dotnet-tasktracker/Program.cs
using Microsoft.EntityFrameworkCore;
using TaskTrackerApi.Data;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllers();

// Configure Entity Framework with SQLite
builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseSqlite(builder.Configuration.GetConnectionString("DefaultConnection")));

// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
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

app.UseAuthorization();

app.MapControllers();

app.Run();
```

## Step 3: Update appsettings.json with Connection String

Add the SQLite connection string to your `appsettings.json`:

```json
// filepath: /workspaces/dotnet-tasktracker/appsettings.json
{
  "ConnectionStrings": {
    "DefaultConnection": "Data Source=tasktracker.db"
  },
  "Logging": {
    "LogLevel": {
      "Default": "Information",
      "Microsoft.AspNetCore": "Warning"
    }
  },
  "AllowedHosts": "*"
}
```

## Step 4: Create Initial Migration

Open the integrated terminal in VS Code and run the following commands:

```bash
# Install EF Core tools globally if not already installed
dotnet tool install --global dotnet-ef

# Create the initial migration
dotnet ef migrations add InitialCreate

# Apply the migration to create the database
dotnet ef database update
```

## Step 5: Verify Setup

1. **Check Migration Files**: You should see a new `Migrations` folder with migration files
2. **Check Database**: A `tasktracker.db` file should be created in your project root
3. **Build and Run**: Test that your application builds and runs without errors:

```bash
dotnet build
dotnet run
```

## Step 6: Optional - Add Development Data Seeding

To make testing easier, you can add some seed data. Create a `DbInitializer.cs` in the `Data` folder:

```csharp
// filepath: /workspaces/dotnet-tasktracker/Data/DbInitializer.cs
using TaskTrackerApi.Models;

namespace TaskTrackerApi.Data
{
    /// <summary>
    /// Database initialization and seeding
    /// </summary>
    public static class DbInitializer
    {
        /// <summary>
        /// Ensures database is created and seeds initial data if needed
        /// </summary>
        public static void Initialize(AppDbContext context)
        {
            context.Database.EnsureCreated();

            // Check if database has been seeded
            if (context.Users.Any())
            {
                return; // Database has been seeded
            }

            // Add seed data if needed for development
            // This is optional for now
        }
    }
}
```

## Verification Checklist

✅ **AppDbContext created** with proper DbSet properties  
✅ **Program.cs updated** with DbContext dependency injection  
✅ **Connection string added** to appsettings.json  
✅ **Migration created** using `dotnet ef migrations add InitialCreate`  
✅ **Database updated** using `dotnet ef database update`  
✅ **Application builds and runs** without errors  
✅ **SQLite database file created** (tasktracker.db)

## Expected Outcome

After completing Day 3, you should have:
- A properly configured EF Core DbContext
- SQLite database with Users and Tasks tables
- Database schema initialized and ready for data
- All necessary dependencies registered in the DI container

Your database should now be ready for the authentication endpoints you'll build on Day 4!