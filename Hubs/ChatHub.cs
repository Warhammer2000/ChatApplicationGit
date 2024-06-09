using ChatApplication.Data;
using ChatApplication.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;

namespace ChatApplication.Hubs
{
    public class ChatHub : Hub
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;

        public ChatHub(ApplicationDbContext context, UserManager<ApplicationUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }
        public async Task SendMessage(string senderId, string receiverId, string message)
        {
            var messageId = Guid.NewGuid().ToString();

            var sender = await _userManager.FindByIdAsync(senderId);
            if (sender == null || string.IsNullOrEmpty(sender.Email))
            {
                return;
            }

            await Clients.Group(receiverId).SendAsync("ReceiveMessage", senderId, message, messageId);
            await Clients.Caller.SendAsync("MessageSent", senderId, message, messageId);
        }
        public async Task StartCall(string receiverId)
        {
            if (string.IsNullOrEmpty(receiverId))
            {
                throw new ArgumentException("Receiver ID is required", nameof(receiverId));
            }

            await Clients.User(receiverId).SendAsync("ReceiveCall", Context.ConnectionId);
        }
        public async Task JoinChat(string userId, string chatId)
        {
            await Groups.AddToGroupAsync(Context.ConnectionId, chatId);
        }

        public async Task LeaveChat(string userId, string chatId)
        {
            await Groups.RemoveFromGroupAsync(Context.ConnectionId, chatId);
        }
        public async Task ReceiveCall(string callerId)
        {
            if (string.IsNullOrEmpty(callerId))
            {
                throw new ArgumentException("Caller ID is required", nameof(callerId));
            }

            await Clients.Caller.SendAsync("CallStarted", callerId);
        }
        public async Task SendSignal(string userId, string signal)
        {
            await Clients.User(userId).SendAsync("ReceiveSignal", signal);
        }
        public async Task SendFile(string senderId, string receiverId, string fileName, string fileUrl)
        {
            await Clients.Group(receiverId).SendAsync("ReceiveFile", senderId, fileName, fileUrl);
            await Clients.Group(senderId).SendAsync("ReceiveFile", senderId, fileName, fileUrl); 
        }
        public async Task DeleteMessage(int messageId)
        {
            var userId = _userManager.GetUserId(Context.User);
            var message = await _context.Messages.FindAsync(messageId);

            if (message != null && message.Sender == userId)
            {
                _context.Messages.Remove(message);
                await _context.SaveChangesAsync();

                await Clients.Group(message.Receiver).SendAsync("DeleteMessage", messageId);
                await Clients.Group(message.Sender).SendAsync("DeleteMessage", messageId);
            }
        }

        public async Task EditMessage(int messageId, string newText)
        {
            var userId = _userManager.GetUserId(Context.User);
            var message = await _context.Messages.FindAsync(messageId);

            if (message != null && message.Sender == userId)
            {
                message.Text = newText;
                _context.Messages.Update(message);
                await _context.SaveChangesAsync();

                await Clients.Group(message.Receiver).SendAsync("EditMessage", messageId, newText);
                await Clients.Group(message.Sender).SendAsync("EditMessage", messageId, newText);
            }
        }
    }
}
