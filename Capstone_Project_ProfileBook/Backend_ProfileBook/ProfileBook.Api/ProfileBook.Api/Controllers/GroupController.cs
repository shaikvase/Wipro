using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ProfileBook.Api.Data;
using ProfileBook.Api.Models;
using ProfileBook.Api.DTOs;
using System.Security.Claims;

namespace ProfileBook.Api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize(Roles = "Admin")] // Only admins can manage groups
    public class GroupController : ControllerBase
    {
        private readonly ApplicationDbContext _context;

        public GroupController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: api/Group - Get all groups
        [HttpGet]
        public async Task<IActionResult> GetGroups()
        {
            var groups = await _context.Groups
                .Include(g => g.GroupMembers)
                .Select(g => new 
                {
                    g.GroupId,
                    g.GroupName,
                    GroupMembers = g.GroupMembers.Select(gm => new { gm.UserId, gm.Username }).ToList()
                })
                .ToListAsync();
            return Ok(groups);
        }

        // POST: api/Group - Create a new group
        [HttpPost]
        public async Task<IActionResult> CreateGroup([FromBody] GroupCreateDto dto)
        {
            if (string.IsNullOrWhiteSpace(dto.GroupName))
            {
                return BadRequest("Group name is required.");
            }

            var group = new Group { GroupName = dto.GroupName };
            _context.Groups.Add(group);
            await _context.SaveChangesAsync();

            // Add selected members
            if (dto.MemberIds != null && dto.MemberIds.Any())
            {
                var members = await _context.Users
                    .Where(u => dto.MemberIds.Contains(u.UserId))
                    .ToListAsync();
                foreach (var member in members)
                {
                    group.GroupMembers.Add(member);
                }
                await _context.SaveChangesAsync();
            }

            return CreatedAtAction(nameof(GetGroups), new { id = group.GroupId }, group);
        }

        // PUT: api/Group/{id}/add-members - Add members to an existing group
        [HttpPut("{id}/add-members")]
        public async Task<IActionResult> AddMembers(int id, [FromBody] GroupMemberUpdateDto dto)
        {
            var group = await _context.Groups
                .Include(g => g.GroupMembers)
                .FirstOrDefaultAsync(g => g.GroupId == id);

            if (group == null)
            {
                return NotFound("Group not found.");
            }

            var usersToAdd = await _context.Users
                .Where(u => dto.MemberIds.Contains(u.UserId) && !group.GroupMembers.Any(gm => gm.UserId == u.UserId))
                .ToListAsync();

            foreach (var user in usersToAdd)
            {
                group.GroupMembers.Add(user);
            }
            await _context.SaveChangesAsync();

            return NoContent();
        }

        // PUT: api/Group/{id}/remove-members - Remove members from an existing group
        [HttpPut("{id}/remove-members")]
        public async Task<IActionResult> RemoveMembers(int id, [FromBody] GroupMemberUpdateDto dto)
        {
            var group = await _context.Groups
                .Include(g => g.GroupMembers)
                .FirstOrDefaultAsync(g => g.GroupId == id);

            if (group == null)
            {
                return NotFound("Group not found.");
            }

            var usersToRemove = group.GroupMembers
                .Where(gm => dto.MemberIds.Contains(gm.UserId))
                .ToList();

            foreach (var user in usersToRemove)
            {
                group.GroupMembers.Remove(user);
            }
            await _context.SaveChangesAsync();

            return NoContent();
        }

        // DELETE: api/Group/{id} - Delete a group
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteGroup(int id)
        {
            var group = await _context.Groups.FindAsync(id);
            if (group == null)
            {
                return NotFound("Group not found.");
            }

            _context.Groups.Remove(group);
            await _context.SaveChangesAsync();

            return NoContent();
        }
    }
}