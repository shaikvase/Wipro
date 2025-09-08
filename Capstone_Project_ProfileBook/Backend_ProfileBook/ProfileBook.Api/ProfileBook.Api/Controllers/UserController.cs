using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ProfileBook.Api.Data;
using ProfileBook.Api.Models;
using ProfileBook.Api.DTOs;
using ProfileBook.Api.Services;
using System.Security.Cryptography;
using System.Text;

namespace ProfileBook.Api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class UserController : ControllerBase
    {
        private readonly ApplicationDbContext _context;
        private readonly AuthService _auth;
        private readonly IConfiguration _cfg;

        public UserController(ApplicationDbContext context, AuthService auth, IConfiguration cfg)
        {
            _context = context;
            _auth = auth;
            _cfg = cfg;
        }

        // ---- Simple SHA256 helper (demo). Prefer BCrypt in production. ----
        private static string HashPassword(string password)
        {
            using var sha = SHA256.Create();
            var bytes = sha.ComputeHash(Encoding.UTF8.GetBytes(password));
            return Convert.ToBase64String(bytes);
        }

        // ------------------- SINGLE REGISTER -------------------
        // Body: { username, password, role: "User"|"Admin", inviteCode?: string }
        [HttpPost("register")]
        public async Task<IActionResult> Register(RegisterDto dto)
        {
            if (string.IsNullOrWhiteSpace(dto.Username) || string.IsNullOrWhiteSpace(dto.Password))
                return BadRequest("Username and password are required.");

            // Normalize/validate role
            var role = (dto.Role ?? "User").Trim();
            if (!string.Equals(role, "User", StringComparison.OrdinalIgnoreCase) &&
                !string.Equals(role, "Admin", StringComparison.OrdinalIgnoreCase))
            {
                return BadRequest("Role must be 'User' or 'Admin'.");
            }

            // If role is Admin, require invite code
            if (role.Equals("Admin", StringComparison.OrdinalIgnoreCase))
            {
                var invite = _cfg["Admin:InviteCode"];
                if (string.IsNullOrWhiteSpace(invite))
                    return StatusCode(500, "Admin invite code not configured by server.");

                if (!string.Equals(dto.InviteCode, invite, StringComparison.Ordinal))
                    return Unauthorized("Invalid admin invite code.");
            }

            // Unique username check
            var exists = await _context.Users.AnyAsync(u => u.Username == dto.Username);
            if (exists) return Conflict("Username already exists.");

            var user = new User
            {
                Username = dto.Username.Trim(),
                PasswordHash = HashPassword(dto.Password),
                Role = role.Equals("Admin", StringComparison.OrdinalIgnoreCase) ? "Admin" : "User",
                ProfileImage = string.Empty
            };

            _context.Users.Add(user);
            await _context.SaveChangesAsync();

            return Ok(new { user.UserId, user.Username, user.Role });
        }

        // --------------------- LOGIN ---------------------
        [HttpPost("login")]
        public async Task<IActionResult> Login(LoginDto dto)
        {
            var user = await _context.Users.FirstOrDefaultAsync(u => u.Username == dto.Username);
            if (user == null) return Unauthorized("Invalid username or password.");

            var hashed = HashPassword(dto.Password);
            if (user.PasswordHash != hashed) return Unauthorized("Invalid username or password.");

            var token = _auth.GenerateJwtToken(user);
            return Ok(new { token, userId = user.UserId, role = user.Role });
        }

        // ------------------- GET PROFILE -------------------
        [HttpGet("{id:int}")]
        public async Task<IActionResult> GetUser(int id)
        {
            var user = await _context.Users
                .AsNoTracking()
                .Where(u => u.UserId == id)
                .Select(u => new { u.UserId, u.Username, u.Role, u.ProfileImage })
                .FirstOrDefaultAsync();

            if (user == null) return NotFound("User not found.");
            return Ok(user);
        }

        // -------------- UPLOAD PROFILE IMAGE --------------
        [HttpPost("upload-profile/{userId:int}")]
        public async Task<IActionResult> UploadProfileImage(int userId, IFormFile file)
        {
            if (file == null || file.Length == 0)
                return BadRequest("No file uploaded.");

            var uploadsFolder = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "uploads");
            if (!Directory.Exists(uploadsFolder))
                Directory.CreateDirectory(uploadsFolder);

            var fileName = $"{Guid.NewGuid()}_{file.FileName}";
            var filePath = Path.Combine(uploadsFolder, fileName);

            using (var stream = new FileStream(filePath, FileMode.Create))
            {
                await file.CopyToAsync(stream);
            }

            var user = await _context.Users.FindAsync(userId);
            if (user == null) return NotFound("User not found.");

            user.ProfileImage = $"/uploads/{fileName}";
            await _context.SaveChangesAsync();

            return Ok(new { message = "Profile image uploaded", path = user.ProfileImage });
        }
                [HttpGet("search")]
        public async Task<IActionResult> SearchUsers([FromQuery] string query)
        {
            if (string.IsNullOrWhiteSpace(query))
            {
                return Ok(new List<User>());
            }

            var users = await _context.Users
                .Where(u => u.Username.ToLower().Contains(query.ToLower()))
                .Select(u => new { u.UserId, u.Username, u.ProfileImage })
                .Take(10) // Limit results to 10 for performance
                .ToListAsync();

            return Ok(users);
        }

                                        [HttpGet]
        public async Task<IActionResult> GetUsers()
        {
            var users = await _context.Users
                .AsNoTracking()
                .Select(u => new { u.UserId, u.Username, u.ProfileImage })
                .ToListAsync();

            return Ok(users);
        }
    }
}
 