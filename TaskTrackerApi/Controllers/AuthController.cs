using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using TaskTrackerApi.Data;
using TaskTrackerApi.Models;
using TaskTrackerApi.DTOs.Auth;
using Microsoft.EntityFrameworkCore;
using TaskTrackerApi.Services;
using AutoMapper;

namespace TaskTrackerApi.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AuthController : ControllerBase
    {
        private readonly AppDbContext _context;
        private readonly IPasswordHasher<User> _passwordHasher;
        private readonly IJwtService _jwtService;
        private readonly IConfiguration _configuration;
        private readonly IMapper _mapper;

        public AuthController(AppDbContext context, IPasswordHasher<User> passwordHasher, IJwtService jwtService, IConfiguration configuration, IMapper mapper)
        {
            _context = context;
            _passwordHasher = passwordHasher;
            _jwtService = jwtService;
            _configuration = configuration;
            _mapper = mapper;
        }

        /// <summary>
        /// Registers a new user.
        /// </summary>
        /// <param name="request">The registration request containing username and password.</param>
        /// <returns>A success message or an error message.</returns>
        [HttpPost("register")]
        public async Task<IActionResult> Register(RegisterRequest request)
        {
            var existingUser = await _context.Users.FirstOrDefaultAsync(u => u.Username == request.Username);

            if (existingUser != null)
            {
                return Conflict(new { message = "Username already exists." });
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

            return Ok(new { message = "User registered successfully.", userId = user.Id });
        }

        [HttpPost("login")]
        public async Task<IActionResult> Login(LoginRequest request)
        {
            var user = await _context.Users.FirstOrDefaultAsync(u => u.Username == request.Username);

            if (user == null)
            {
                return Unauthorized(new { message = "Invalid username or password." });
            }

            var verificationResult = _passwordHasher.VerifyHashedPassword(user, user.PasswordHash, request.Password);

            if (verificationResult == PasswordVerificationResult.Failed)
            {
                return Unauthorized(new { message = "Invalid username or password." });
            }

            // Generate JWT token
            var token = _jwtService.GenerateToken(user);
            var expiryInMinutes = Convert.ToDouble(_configuration["JwtSettings:ExpiryInMinutes"]);
            // var response = new AuthResponse
            // {
            //     Token = token,
            //     Username = user.Username,
            //     ExpiresAt = DateTime.UtcNow.AddMinutes(expiryInMinutes)
            // };

            var response = _mapper.Map<AuthResponse>(user);
            response.Token = token;
            response.ExpiresAt = DateTime.UtcNow.AddMinutes(expiryInMinutes);

            return Ok(response);
        }
    }
}