using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ProfileBook.Api.Data;
using ProfileBook.Api.DTOs;
using ProfileBook.Api.Models;
using System.IdentityModel.Tokens.Jwt; // Required for JwtRegisteredClaimNames
using System.Security.Claims;
using ProfileBook.Api.Hubs; 
using Microsoft.AspNetCore.SignalR;

namespace ProfileBook.Api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize] // Secures all methods in this controller, requiring a valid JWT
    public class PostController : ControllerBase
    {
        private readonly ApplicationDbContext _context;
        private readonly IHubContext<PostHub> _hubContext; 
        public PostController(ApplicationDbContext context, IHubContext<PostHub> hubContext)
        {
            _context = context;
            _hubContext = hubContext; // This line will now work correctly
        }

        /// <summary>
        /// A private helper method to safely get the current user's ID from their token.
        /// </summary>
        /// <returns>The integer UserId, or 0 if not found.</returns>
        private int GetCurrentUserId()
        {
            // The user's ID is stored in the "NameIdentifier" claim in the JWT
            var userIdString = User.FindFirstValue(ClaimTypes.NameIdentifier);
            int.TryParse(userIdString, out var userId);
            return userId;
        }

        /// <summary>
        /// Gets the post feed. Admins see all posts; regular users see only "Approved" posts.
        /// Also includes like counts, comments, and whether the current user liked each post.
        /// </summary>
        [HttpGet]
        public async Task<IActionResult> GetPosts([FromQuery] string? searchQuery, [FromQuery] string? statusFilter)
        {
            var currentUserId = GetCurrentUserId();
            var isAdmin = User.IsInRole("Admin");
            var query = _context.Posts.AsQueryable();

            // Apply status filter
            if (!string.IsNullOrEmpty(statusFilter))
            {
                // Admins can filter by any status, regular users can only see 'Approved'
                if (isAdmin || statusFilter.Equals("Approved", StringComparison.OrdinalIgnoreCase))
                {
                    query = query.Where(p => p.Status == statusFilter);
                }
                else if (!isAdmin && !statusFilter.Equals("Approved", StringComparison.OrdinalIgnoreCase))
                {
                    // Regular users cannot filter by non-approved statuses
                    return Forbid("Regular users can only filter by 'Approved' status.");
                }
            }
            // If no statusFilter is provided AND there is no searchQuery AND user is not admin, default to Approved
            else if (!isAdmin && string.IsNullOrEmpty(searchQuery))
            {
                query = query.Where(p => p.Status == "Approved");
            }

            // Apply search query
            if (!string.IsNullOrEmpty(searchQuery))
            {
                query = query.Where(p =>
                    p.Content.Contains(searchQuery) ||
                    p.User.Username.Contains(searchQuery)); // Search by content or username
            }

            var posts = await query
                .Include(p => p.User)
                .Include(p => p.Likes)
                .Include(p => p.Comments)
                    .ThenInclude(c => c.User)
                .Select(p => new PostResponseDto
                {
                    PostId = p.PostId,
                    UserId = p.UserId,
                    Username = p.User.Username,
                    ProfileImage = p.User.ProfileImage,
                    Content = p.Content,
                    PostImage = p.PostImage,
                    Status = p.Status,
                    LikeCount = p.Likes.Count,
                    IsLikedByCurrentUser = p.Likes.Any(l => l.UserId == currentUserId),
                    Comments = p.Comments.Select(c => new CommentDto
                    {
                        CommentId = c.CommentId,
                        Content = c.Content,
                        Username = c.User.Username,
                        ProfileImage = c.User.ProfileImage,
                        Timestamp = c.Timestamp
                    }).OrderBy(c => c.Timestamp).ToList()
                })
                .OrderByDescending(p => p.PostId)
                .ToListAsync();
                
            return Ok(posts);
        }

        /// <summary>
        /// Creates a new post. The status defaults to "Pending".
        /// </summary>
        [HttpPost]
        public async Task<IActionResult> CreatePost(PostDto dto)
        {
            var currentUserId = GetCurrentUserId();
            if (currentUserId == 0) return Unauthorized();

            var post = new Post
            {
                UserId = currentUserId,
                Content = dto.Content,
                PostImage = dto.PostImage ?? string.Empty
            };
            
            _context.Posts.Add(post);
            await _context.SaveChangesAsync();
            await _hubContext.Clients.All.SendAsync("NewPostPending");

            return Ok(post);
        }
        
        /// <summary>
        /// Uploads an image for an existing post.
        /// </summary>
        [HttpPost("upload-image/{postId:int}")]
        public async Task<IActionResult> UploadPostImage(int postId, IFormFile file)
        {
            if (file == null || file.Length == 0)
                return BadRequest("No file uploaded.");

            var post = await _context.Posts.FindAsync(postId);
            if (post == null)
                return NotFound("Post not found.");

            var uploadsFolder = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "uploads");
            if (!Directory.Exists(uploadsFolder))
                Directory.CreateDirectory(uploadsFolder);

            var fileName = $"{Guid.NewGuid()}_{file.FileName}";
            var filePath = Path.Combine(uploadsFolder, fileName);

            using (var stream = new FileStream(filePath, FileMode.Create))
            {
                await file.CopyToAsync(stream);
            }

            post.PostImage = $"/uploads/{fileName}";
            await _context.SaveChangesAsync();
              var newLikeCount = await _context.Likes.CountAsync(l => l.PostId == postId);
            await _hubContext.Clients.All.SendAsync("ReceiveNewLike", postId, newLikeCount);
            return Ok(new { message = "Post image uploaded", path = post.PostImage });
        }

        /// <summary>
        /// Likes or unlikes a post for the current user.
        /// </summary>
        [HttpPost("{postId}/like")]
        public async Task<IActionResult> ToggleLike(int postId)
        {
            var currentUserId = GetCurrentUserId();
            if (currentUserId == 0) return Unauthorized();
            
            var existingLike = await _context.Likes
                .FirstOrDefaultAsync(l => l.PostId == postId && l.UserId == currentUserId);

            if (existingLike != null)
            {
                _context.Likes.Remove(existingLike);
            }
            else
            {
                var newLike = new Like { PostId = postId, UserId = currentUserId };
                _context.Likes.Add(newLike);
            }

            await _context.SaveChangesAsync();

            var post = await _context.Posts
                .Include(p => p.User)
                .Include(p => p.Likes)
                .Include(p => p.Comments)
                    .ThenInclude(c => c.User)
                .Where(p => p.PostId == postId)
                .Select(p => new PostResponseDto
                {
                    PostId = p.PostId,
                    UserId = p.UserId,
                    Username = p.User.Username,
                    Content = p.Content,
                    PostImage = p.PostImage,
                    Status = p.Status,
                    LikeCount = p.Likes.Count,
                    IsLikedByCurrentUser = p.Likes.Any(l => l.UserId == currentUserId),
                    Comments = p.Comments.Select(c => new CommentDto
                    {
                        CommentId = c.CommentId,
                        Content = c.Content,
                        Username = c.User.Username,
                        Timestamp = c.Timestamp
                    }).OrderBy(c => c.Timestamp).ToList()
                })
                .FirstAsync();

            await _hubContext.Clients.All.SendAsync("ReceiveNewLike", postId, post.LikeCount);

            return Ok(post);
        }

        /// <summary>
        /// Adds a new comment to a post.
        /// </summary>
        // --- THIS IS THE CORRECTED METHOD ---
        [HttpPost("{postId}/comment")]
        public async Task<IActionResult> AddComment(int postId, [FromBody] CommentCreateDto dto)
        {
            var currentUserId = GetCurrentUserId();
            if (currentUserId == 0) return Unauthorized();

            var postExists = await _context.Posts.AnyAsync(p => p.PostId == postId);
            if (!postExists)
            {
                return NotFound("Post not found.");
            }

            var comment = new Comment
            {
                Content = dto.Content,
                PostId = postId,
                UserId = currentUserId,
                Timestamp = DateTime.UtcNow
            };
            
            _context.Comments.Add(comment);
            await _context.SaveChangesAsync();
            
            // --- START OF FIX ---
            // After saving, get the user who made the comment to ensure we have the correct username.
            var user = await _context.Users.FindAsync(currentUserId);
            // --- END OF FIX ---

            var createdCommentDto = new CommentDto
            {
                CommentId = comment.CommentId,
                Content = comment.Content,
                // --- THIS LINE IS NOW CORRECTED ---
                // Use the username we just fetched from the database.
                Username = user?.Username ?? "Unknown", 
                Timestamp = comment.Timestamp
            };
            await _hubContext.Clients.All.SendAsync("ReceiveNewComment", postId, createdCommentDto);

            return Ok(createdCommentDto);
        }

        [HttpDelete("{id}")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> DeletePost(int id)
        {
            var post = await _context.Posts.FindAsync(id);
            if (post == null)
            {
                return NotFound("Post not found.");
            }

            // Find and delete all reports associated with this post
            var reports = await _context.Reports.Where(r => r.PostId == id).ToListAsync();
            _context.Reports.RemoveRange(reports);

            // Find and delete all likes associated with this post
            var likes = await _context.Likes.Where(l => l.PostId == id).ToListAsync();
            _context.Likes.RemoveRange(likes);

            // Find and delete all comments associated with this post
            var comments = await _context.Comments.Where(c => c.PostId == id).ToListAsync();
            _context.Comments.RemoveRange(comments);

            _context.Posts.Remove(post);
            await _context.SaveChangesAsync();

            await _hubContext.Clients.All.SendAsync("PostDeleted", id);

            return NoContent();
        }
    }
}