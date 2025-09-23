# GitHub Copilot Instructions for TaskTracker API

## Project Overview
- **App Name:** TaskTracker API
- **Purpose:** Provide a RESTful API for managing tasks, including creating, reading, updating, and deleting tasks.
- **Tech Stack:** 
  - .NET 8 / .NET Core
  - Entity Framework (EF) Core with SQLite
  - JWT Authentication using Microsoft.AspNetCore.Authentication.JwtBearer
  - Swagger for API testing and documentation

## Project Structure
- `Controllers/` - API controllers (Tasks, Users, Auth, etc.)
- `Models/` - Entity classes (Task, User)
- `DTOs/` - Data Transfer Objects for request and response
- `Data/` - EF Core DbContext
- `Services/` - Business logic and helper services
- `Program.cs` - App configuration, middleware, JWT setup
- `appsettings.json` - Database and JWT configuration
- `Migrations/` - EF Core migrations for SQLite database

## Key Features to Implement
1. **User Authentication**
   - JWT-based authentication
   - Registration and login endpoints
   - Secure password storage (hashing)
2. **Task Management**
   - CRUD endpoints for tasks
   - Task properties: `Id`, `Title`, `Description`, `IsCompleted`, `CreatedAt`, `UpdatedAt`, `UserId`
   - Task ownership: each task is associated with a user
3. **Database**
   - SQLite for simplicity
   - EF Core migrations for schema
4. **Swagger**
   - Document all endpoints
   - Enable JWT authentication in Swagger UI
5. **Error Handling**
   - Standardized error responses
   - 404 for missing tasks, 401 for unauthorized access

## Coding Guidelines
- Follow C# conventions and clean code principles.
- Use async/await for all database calls.
- Use dependency injection for services and repositories.
- Include XML comments for public methods and controllers.
- Write unit tests for services and controllers (optional but recommended).

## Copilot Suggestions
- Generate controllers for CRUD operations with proper route attributes.
- Suggest DTOs and mapping methods.
- Suggest EF Core queries for task management.
- Generate JWT authentication setup snippets in `Program.cs`.
- Suggest middleware for error handling and logging.
- Generate Swagger configuration snippets for JWT support.

## Helpful Notes
- Use `[Authorize]` attribute on endpoints that require authentication.
- Use `Include` or `ThenInclude` in EF Core for eager loading of related entities.
- Seed initial data if necessary for testing in SQLite.
