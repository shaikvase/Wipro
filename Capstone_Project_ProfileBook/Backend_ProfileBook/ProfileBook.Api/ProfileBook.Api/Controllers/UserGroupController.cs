using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ProfileBook.Api.Data;
using ProfileBook.Api.Models;
using System.Security.Claims;
using System.Linq;
using System.Threading.Tasks;

namespace ProfileBook.Api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize] // Authorized for any logged-in user
    public class UserGroupController : ControllerBase
    {
        private readonly ApplicationDbContext _context;

        public UserGroupController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: api/UserGroup/my-groups - Get groups for the current user
        [HttpGet("my-groups")]
        public async Task<IActionResult> GetMyGroups()
        {
            var currentUserId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier));

            var groups = await _context.Groups
                .Where(g => g.GroupMembers.Any(gm => gm.UserId == currentUserId))
                .Select(g => new
                {
                    g.GroupId,
                    g.GroupName,
                    GroupMembers = g.GroupMembers.Select(gm => new { gm.UserId, gm.Username }).ToList()
                })
                .ToListAsync();

            return Ok(groups);
        }
    }
}