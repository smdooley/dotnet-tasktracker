Absolutely! Here's a **10-day challenge** (each day <1 hour) to build a **Task Management RESTful API** using **.NET Core**, **Entity Framework**, and **JWT authentication**. Each task builds incrementally so you’ll have a fully functional, secure, and well-structured API by the end.

---

### 🔧 Overview:

* **App Name:** TaskTracker API
* **Tech Stack:** .NET 8 / .NET Core, EF Core, JWT (via Microsoft.AspNetCore.Authentication.JwtBearer), SQLite (for simplicity), Swagger for testing.

---

## ✅ Day-by-Day Challenge

### **🗓 Day 1 – Project Setup & Initial Config**

**Goal:** Create the base project structure
**Tasks:**

* Scaffold a new ASP.NET Core Web API:

  ```bash
  dotnet new webapi -n TaskTrackerApi
  cd TaskTrackerApi
  ```
* Add EF Core and SQLite NuGet packages:

  ```bash
  dotnet add package Microsoft.EntityFrameworkCore.Sqlite
  dotnet add package Microsoft.EntityFrameworkCore.Tools
  ```
* Setup initial `appsettings.json` with connection string
* Enable Swagger in development mode

> Deliverable: API builds and runs; Swagger UI loads

---

### **🗓 Day 2 – Create the Database Models**

**Goal:** Define your core data models
**Tasks:**

* Create `User` model with Id, Username, PasswordHash
* Create `TaskItem` model with Id, Title, Description, DueDate, IsCompleted, and UserId (FK)

> Deliverable: Two clean models with navigation properties

---

### **🗓 Day 3 – Set Up EF Core DbContext**

**Goal:** Setup database access
**Tasks:**

* Create `AppDbContext` class with `DbSet<User>` and `DbSet<TaskItem>`
* Configure the context in `Program.cs` with dependency injection
* Enable migrations:

  ```bash
  dotnet ef migrations add InitialCreate
  dotnet ef database update
  ```

> Deliverable: DB schema initialized in SQLite

---

### **🗓 Day 4 – User Registration Endpoint**

**Goal:** Allow new users to register
**Tasks:**

* Add `AuthController`
* Add DTO for `RegisterRequest`
* Implement password hashing with `PasswordHasher<T>`
* Save user to DB

> Deliverable: `POST /api/auth/register` creates user in DB

---

### **🗓 Day 5 – JWT Authentication Setup**

**Goal:** Secure your API
**Tasks:**

* Add JWT NuGet:

  ```bash
  dotnet add package Microsoft.AspNetCore.Authentication.JwtBearer
  ```
* Configure JWT in `Program.cs`
* Create token generation service
* Implement login endpoint

> Deliverable: `POST /api/auth/login` returns a valid JWT token

---

### **🗓 Day 6 – Protect Endpoints with Authorization**

**Goal:** Restrict API access to authenticated users
**Tasks:**

* Decorate `TaskController` with `[Authorize]`
* Configure middleware to validate JWT
* Test login → copy token → access protected route

> Deliverable: Unauthorized requests are blocked; JWT-authenticated requests succeed

---

### **🗓 Day 7 – Implement CRUD for Task Items**

**Goal:** Let users manage their tasks
**Tasks:**

* Create `TaskController`
* Implement:

  * `GET /tasks`
  * `GET /tasks/{id}`
  * `POST /tasks`
  * `PUT /tasks/{id}`
  * `DELETE /tasks/{id}`
* Filter tasks by the logged-in user (`UserId` from token)

> Deliverable: Complete authenticated CRUD for task items

---

### **🗓 Day 8 – Add DTOs and AutoMapper**

**Goal:** Decouple internal models from API responses
**Tasks:**

* Create DTOs for Task requests/responses
* Install and configure AutoMapper:

  ```bash
  dotnet add package AutoMapper.Extensions.Microsoft.DependencyInjection
  ```
* Replace manual mappings with AutoMapper

> Deliverable: Clean separation between domain models and exposed API

---

### **🗓 Day 9 – Improve Validation & Error Handling**

**Goal:** Add robust input validation and error responses
**Tasks:**

* Use `[ApiController]` and data annotations for model validation
* Return `BadRequest`, `NotFound`, `Unauthorized` appropriately
* Add global exception handler middleware (optional)

> Deliverable: Clear, consistent API responses for good/bad requests

---

### **🗓 Day 10 – Final Polish & Swagger Auth**

**Goal:** Final testing and polish
**Tasks:**

* Add JWT bearer authentication support to Swagger UI
* Add `CreatedAtAction` to `POST /tasks` for RESTful compliance
* Add sorting or filtering query parameters (optional)
* Review and test full flow: Register → Login → CRUD Tasks

> Deliverable: Fully functional, testable, and secure Task Management API

---

## 🎉 Bonus Ideas (Optional Post-Challenge)

* Add refresh tokens
* Add user roles (e.g. Admin)
* Deploy to Azure or Railway
* Use SQL Server instead of SQLite
* Add pagination for tasks

---

Would you like me to generate this as a downloadable checklist or GitHub project board?
