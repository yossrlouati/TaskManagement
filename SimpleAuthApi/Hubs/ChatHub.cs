using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;
using SimpleAuthApi.Data;
using SimpleAuthApi.Models;

namespace SimpleAuthApi.Hubs
{
    [Authorize]
    public class ChatHub : Hub
    {
        private readonly AppDbContext _context;
        // Liste statique des utilisateurs connectés (en mémoire RAM du serveur)
        // Note : En vie pro avec plusieurs serveurs, on utiliserait Redis ici !
        private static readonly HashSet<string> _onlineUsers = new HashSet<string>();

        public ChatHub(AppDbContext context)
        {
            _context = context;
        }
        // 1. Quand un utilisateur se connecte
        public override async Task OnConnectedAsync()
        {
            var userId = Context.UserIdentifier;
            if (!string.IsNullOrEmpty(userId))
            {
                lock (_onlineUsers)
                {
                    _onlineUsers.Add(userId);
                }
                // Optionnel : Prévenir les autres que cet utilisateur est en ligne
                await Clients.All.SendAsync("UserStatusChanged", userId, true);
            }
            await base.OnConnectedAsync();
        }

        // 2. Quand un utilisateur se déconnecte
        public override async Task OnDisconnectedAsync(Exception? exception)
        {
            var userId = Context.UserIdentifier;
            if (!string.IsNullOrEmpty(userId))
            {
                lock (_onlineUsers)
                {
                    _onlineUsers.Remove(userId);
                }
                // Prévenir les autres que cet utilisateur est hors ligne
                await Clients.All.SendAsync("UserStatusChanged", userId, false);
            }
            await base.OnDisconnectedAsync(exception);
        }

        public async Task SendPrivateMessage(string receiverId, string message)
        {
            var senderId = Context.UserIdentifier;
            var chatMsg = new ChatMessage
            {
                SenderId = senderId,
                ReceiverId = receiverId,
                Message = message,
                SentAt = DateTime.UtcNow
            };
            _context.ChatMessages.Add(chatMsg);
            await _context.SaveChangesAsync();

            await Clients.User(receiverId).SendAsync("ReceiveMessage", senderId, message);
            await Clients.Caller.SendAsync("ReceiveMessage", senderId, message);
        }

        // Nouvelle petite méthode pour que le Front puisse demander qui est en ligne
        public Task<List<string>> GetOnlineUsers()
        {
            lock (_onlineUsers)
            {
                return Task.FromResult(_onlineUsers.ToList());
            }
        }
    }
}

