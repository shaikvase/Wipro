using Microsoft.AspNetCore.SignalR;
using ProfileBook.Api.DTOs;

namespace ProfileBook.Api.Hubs
{
    // This hub will handle real-time updates for posts
    public class PostHub : Hub
    {
        // We will call these methods from our PostController to broadcast updates
        public async Task BroadcastNewLike(int postId, int newLikeCount)
        {
            await Clients.All.SendAsync("ReceiveNewLike", postId, newLikeCount);
        }

        public async Task BroadcastNewComment(int postId, CommentDto newComment)
        {
            await Clients.All.SendAsync("ReceiveNewComment", postId, newComment);
        }
         public async Task NewPostPending()
        {
            await Clients.All.SendAsync("ReceiveNewPostPending");
        }

        public async Task NotifyPostApproved(string userId, int postId)
        {
            await Clients.User(userId).SendAsync("PostApproved", postId);
        }
    }
}