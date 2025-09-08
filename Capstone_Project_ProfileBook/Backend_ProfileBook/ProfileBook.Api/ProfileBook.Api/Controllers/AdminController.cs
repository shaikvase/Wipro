using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ProfileBook.Api.Data;
using ProfileBook.Api.DTOs;
using ProfileBook.Api.Models;
using System.Security.Cryptography;
using System.Text;
using Microsoft.AspNetCore.SignalR;
using ProfileBook.Api.Hubs;

namespace ProfileBook.Api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize(Roles = "Admin")]
    public class AdminController : ControllerBase
    {
        private readonly ApplicationDbContext _context;
        private readonly IHubContext<PostHub> _hubContext;

        public AdminController(ApplicationDbContext context, IHubContext<PostHub> hubContext)
        {
            _context = context;
            _hubContext = hubContext;
        }
        
        private static string HashPassword(string password)
        {
            using var sha = SHA256.Create();
            var bytes = sha.ComputeHash(Encoding.UTF8.GetBytes(password));
            return Convert.ToBase64String(bytes);
        }

        // --- User Management (Full CRUD) ---

        [HttpGet("users")]
        public async Task<IActionResult> GetAllUsers()
        {
            var users = await _context.Users
                .Select(u => new { u.UserId, u.Username, u.Role })
                .ToListAsync();
            return Ok(users);
        }

        [HttpPost("users")]
        public async Task<IActionResult> CreateUser(AdminCreateUserDto dto)
        {
            if (await _context.Users.AnyAsync(u => u.Username == dto.Username))
            {
                return Conflict("Username already exists.");
            }
            var user = new User
            {
                Username = dto.Username,
                PasswordHash = HashPassword(dto.Password),
                Role = dto.Role,
                ProfileImage = ""
            };
            _context.Users.Add(user);
            await _context.SaveChangesAsync();
            return Ok(new { user.UserId, user.Username, user.Role });
        }

        [HttpPut("users/{id}")]
        public async Task<IActionResult> UpdateUser(int id, AdminUpdateUserDto dto)
        {
            var user = await _context.Users.FindAsync(id);
            if (user == null) return NotFound();
            user.Username = dto.Username;
            user.Role = dto.Role;
            _context.Users.Update(user);
            await _context.SaveChangesAsync();
            return Ok(new { user.UserId, user.Username, user.Role });
        }

        [HttpDelete("users/{id}")]
        public async Task<IActionResult> DeleteUser(int id)
        {
            var user = await _context.Users.FindAsync(id);
            if (user == null) return NotFound();
            _context.Users.Remove(user);
            await _context.SaveChangesAsync();
            return Ok("User deleted");
        }

        // --- Post Management ---

        [HttpGet("posts/all")]
        public async Task<IActionResult> GetAllPosts()
        {
            var allPosts = await _context.Posts
                .Include(p => p.User)
                .Select(p => new PostResponseDto
                {
                    PostId = p.PostId,
                    UserId = p.UserId,
                    Username = p.User.Username,
                    Content = p.Content,
                    PostImage = p.PostImage,
                    Status = p.Status,
                    LikeCount = p.Likes.Count(),
                    Comments = new List<CommentDto>()
                })
                .OrderByDescending(p => p.PostId)
                .ToListAsync();
            return Ok(allPosts);
        }

        [HttpPut("posts/approve/{postId}")]
        public async Task<IActionResult> ApprovePost(int postId)
        {
            var post = await _context.Posts.FindAsync(postId);
            if (post == null) return NotFound();
            post.Status = "Approved";
            await _context.SaveChangesAsync();

            await _hubContext.Clients.All.SendAsync("PostStatusChanged", post.PostId, post.Status);

            return Ok(post);
        }

        [HttpPut("posts/reject/{postId}")]
        public async Task<IActionResult> RejectPost(int postId)
        {
            var post = await _context.Posts.FindAsync(postId);
            if (post == null) return NotFound();
            post.Status = "Rejected";
            await _context.SaveChangesAsync();
            await _hubContext.Clients.All.SendAsync("PostStatusChanged", post.PostId, post.Status);
            return Ok(post);
        }

        // --- Reported Users ---

        [HttpGet("reports")]
        public async Task<IActionResult> GetReportedUsers()
        {
            var reports = await _context.Reports
                .Include(r => r.ReportedUser)
                .Include(r => r.ReportingUser)
                .Select(r => new {
                    r.ReportId,
                    r.Reason,
                    r.TimeStamp,
                    ReportedUser = new { r.ReportedUser.UserId, r.ReportedUser.Username },
                    ReportingUser = new { r.ReportingUser.UserId, r.ReportingUser.Username }
                })
                .ToListAsync();
            return Ok(reports);
        }

        // --- Group Management ---

        [HttpPost("groups")]
        public async Task<IActionResult> CreateGroup([FromBody] Group group)
        {
            _context.Groups.Add(group);
            await _context.SaveChangesAsync();
            return Ok(group);
        }

        [HttpGet("groups")]
        public async Task<IActionResult> GetGroups()
        {
            var groups = await _context.Groups.ToListAsync();
            return Ok(groups);
        }
    }
}