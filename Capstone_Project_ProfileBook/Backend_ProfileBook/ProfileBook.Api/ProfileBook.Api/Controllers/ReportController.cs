using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ProfileBook.Api.Data;
using ProfileBook.Api.DTOs;
using ProfileBook.Api.Models;
using System.Security.Claims;
using Microsoft.AspNetCore.SignalR;
using ProfileBook.Api.Hubs;

namespace ProfileBook.Api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class ReportController : ControllerBase
    {
        private readonly ApplicationDbContext _context;
        private readonly IHubContext<PostHub> _hubContext;

        public ReportController(ApplicationDbContext context, IHubContext<PostHub> hubContext)
        {
            _context = context;
            _hubContext = hubContext;
        }

        [HttpPost]
        public async Task<IActionResult> CreateReport(ReportDto dto)
        {
            var reportingUserIdString = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (!int.TryParse(reportingUserIdString, out var reportingUserId))
            {
                return Unauthorized();
            }

            if (dto.ReportedUserId == null && dto.PostId == null)
            {
                return BadRequest("Either ReportedUserId or PostId must be provided.");
            }

            if (dto.ReportedUserId != null && dto.PostId != null)
            {
                return BadRequest("Cannot report both a user and a post in the same request.");
            }

            var report = new Report
            {
                Reason = dto.Reason,
                ReportingUserId = reportingUserId,
                TimeStamp = DateTime.UtcNow
            };

            if (dto.ReportedUserId != null)
            {
                report.ReportedUserId = dto.ReportedUserId.Value;
            }
            else if (dto.PostId != null)
            {
                report.PostId = dto.PostId.Value;
            }

            _context.Reports.Add(report);
            await _context.SaveChangesAsync();

            // Notify all clients about the new report
            await _hubContext.Clients.All.SendAsync("NewReport", report);

            return Ok(report);
        }
    [HttpGet]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> GetReports()
        {
            var reports = await _context.Reports
                .Include(r => r.ReportedUser)
                .Include(r => r.ReportingUser)
                .Include(r => r.Post)
                .Select(r => new
                {
                    r.ReportId,
                    r.Reason,
                    r.TimeStamp,
                    r.IsResolved,
                    ReportedUser = r.ReportedUser != null ? new { r.ReportedUser.UserId, r.ReportedUser.Username } : null,
                    ReportedPost = r.Post != null ? new { r.Post.PostId, r.Post.Content } : null,
                    ReportingUser = new { r.ReportingUser.UserId, r.ReportingUser.Username }
                })
                .ToListAsync();

            return Ok(reports);
        }

        [HttpPut("{id}/resolve")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> ResolveReport(int id)
        {
            var report = await _context.Reports.FindAsync(id);
            if (report == null)
            {
                return NotFound("Report not found.");
            }

            report.IsResolved = true;
            await _context.SaveChangesAsync();

            return NoContent();
        }
    }
}