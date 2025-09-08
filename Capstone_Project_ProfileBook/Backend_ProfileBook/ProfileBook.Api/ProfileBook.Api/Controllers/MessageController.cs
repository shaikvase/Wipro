using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ProfileBook.Api.Data;
using ProfileBook.Api.Models;
using ProfileBook.Api.DTOs;
using System.Security.Claims;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.SignalR;
using ProfileBook.Api.Hubs;

namespace ProfileBook.Api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class MessageController : ControllerBase
    {
        private readonly ApplicationDbContext _context;
        private readonly IHubContext<PostHub> _hubContext;

        public MessageController(ApplicationDbContext context, IHubContext<PostHub> hubContext)
        {
            _context = context;
            _hubContext = hubContext;
        }

        [HttpPost]
        public async Task<IActionResult> SendMessage([FromBody] MessageCreateDto msgDto)
        {
            var senderId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier));

            if (msgDto.ReceiverId == null && msgDto.GroupId == null)
            {
                return BadRequest("Either ReceiverId or GroupId must be provided.");
            }

            if (msgDto.ReceiverId != null && msgDto.GroupId != null)
            {
                return BadRequest("Cannot send a message to both a user and a group.");
            }

            var msg = new Message
            {
                SenderId = senderId,
                MessageContent = msgDto.Content,
                TimeStamp = System.DateTime.UtcNow,
                IsRead = false
            };

            if (msgDto.ReceiverId != null)
            {
                msg.ReceiverId = msgDto.ReceiverId.Value;
            }
            else if (msgDto.GroupId != null)
            {
                // Verify sender is a member of the group
                var isMember = await _context.Groups
                    .Where(g => g.GroupId == msgDto.GroupId.Value)
                    .SelectMany(g => g.GroupMembers)
                    .AnyAsync(u => u.UserId == senderId);

                if (!isMember)
                {
                    return Forbid("You are not a member of this group.");
                }
                msg.GroupId = msgDto.GroupId.Value;
            }

            _context.Messages.Add(msg);
            await _context.SaveChangesAsync();

            // Retrieve the message with sender details for the response
            var messageToReturn = await _context.Messages
                .Include(m => m.Sender)
                .Where(m => m.MessageId == msg.MessageId)
                .Select(m => new
                {
                    m.MessageId,
                    m.SenderId,
                    m.ReceiverId,
                    m.GroupId,
                    m.MessageContent,
                    m.TimeStamp,
                    m.IsRead,
                    Sender = new { m.Sender.UserId, m.Sender.Username, m.Sender.ProfileImage }
                })
                .FirstOrDefaultAsync();

            // SignalR notification
            if (messageToReturn != null)
            {
                if (messageToReturn.ReceiverId != null)
                {
                    // Private message: notify sender and receiver
                    await _hubContext.Clients.User(senderId.ToString()).SendAsync("ReceiveMessage", messageToReturn);
                    await _hubContext.Clients.User(messageToReturn.ReceiverId.ToString()).SendAsync("ReceiveMessage", messageToReturn);
                }
                else if (messageToReturn.GroupId != null)
                {
                    // Group message: notify all group members
                    var groupMembers = await _context.Groups
                        .Where(g => g.GroupId == messageToReturn.GroupId)
                        .SelectMany(g => g.GroupMembers)
                        .Select(u => u.UserId.ToString())
                        .ToListAsync();
                    await _hubContext.Clients.Users(groupMembers).SendAsync("ReceiveMessage", messageToReturn);
                }
            }

            return Ok(messageToReturn);
        }

        [HttpGet("{otherUserId}")]
        public async Task<IActionResult> GetMessageHistory(int otherUserId)
        {
            var currentUserId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier));

            var messages = await _context.Messages
                .Where(m => (m.ReceiverId == currentUserId && m.SenderId == otherUserId) || (m.ReceiverId == otherUserId && m.SenderId == currentUserId))
                .Include(m => m.Sender) // Add this
                .Include(m => m.Receiver) // Add this
                .OrderBy(m => m.TimeStamp)
                .ToListAsync();

            // Mark messages as read when history is fetched
            var unreadMessages = messages.Where(m => m.ReceiverId == currentUserId && !m.IsRead).ToList();
            foreach (var message in unreadMessages)
            {
                message.IsRead = true;
            }
            await _context.SaveChangesAsync();

            if (unreadMessages.Any())
            {
                // Notify sender that messages have been read
                var senderIds = unreadMessages.Select(m => m.SenderId.ToString()).Distinct().ToList();
                await _hubContext.Clients.Users(senderIds).SendAsync("MessagesRead", currentUserId, unreadMessages.Select(m => m.MessageId).ToList());
            }

            // Notify the current user that their unread count might have changed
            await _hubContext.Clients.User(currentUserId.ToString()).SendAsync("UnreadCountChanged");

            return Ok(messages);
        }

        [HttpGet("group/{groupId}")]
        public async Task<IActionResult> GetGroupMessageHistory(int groupId)
        {
            var currentUserId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier));

            // Verify current user is a member of the group
            var isMember = await _context.Groups
                .Where(g => g.GroupId == groupId)
                .SelectMany(g => g.GroupMembers)
                .AnyAsync(u => u.UserId == currentUserId);

            if (!isMember)
            {
                return Forbid("You are not a member of this group.");
            }

            var messages = await _context.Messages
                .Where(m => m.GroupId == groupId)
                .Include(m => m.Sender) // Include sender to display username
                .OrderBy(m => m.TimeStamp)
                .ToListAsync();

            // Mark group messages as read for the current user
            var unreadGroupMessages = messages.Where(m => m.GroupId == groupId && m.SenderId != currentUserId && !m.IsRead).ToList();
            foreach (var message in unreadGroupMessages)
            {
                message.IsRead = true;
            }
            await _context.SaveChangesAsync();

            if (unreadGroupMessages.Any())
            {
                // Notify other group members that messages have been read by this user
                var groupMembers = await _context.Groups
                    .Where(g => g.GroupId == groupId)
                    .SelectMany(g => g.GroupMembers)
                    .Where(u => u.UserId != currentUserId)
                    .Select(u => u.UserId.ToString())
                    .ToListAsync();
                await _hubContext.Clients.Users(groupMembers).SendAsync("GroupMessagesRead", groupId, currentUserId, unreadGroupMessages.Select(m => m.MessageId).ToList());
            }

            // Notify the current user that their unread count might have changed
            await _hubContext.Clients.User(currentUserId.ToString()).SendAsync("UnreadCountChanged");

            return Ok(messages);
        }

        [HttpGet("unreadCount")]
        public async Task<IActionResult> GetUnreadMessageCount()
        {
            var currentUserId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier));
            var unreadCount = await _context.Messages
                .Where(m => m.ReceiverId == currentUserId && !m.IsRead)
                .CountAsync();
            return Ok(unreadCount);
        }
    }
}