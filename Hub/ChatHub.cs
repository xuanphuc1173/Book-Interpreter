using EXE.Controllers;
using Microsoft.AspNetCore.SignalR;
using System.Collections.Concurrent;
using System.Threading.Tasks;

namespace EXE.Hubs
{
    public class ChatHub : Hub
    {
        private readonly IHttpContextAccessor _httpContextAccessor;
        private static readonly ConcurrentDictionary<string, string> ConnectedUsers = new();

        public ChatHub(IHttpContextAccessor httpContextAccessor)
        {
            _httpContextAccessor = httpContextAccessor;
        }
        // Phương thức này được gọi khi người dùng kết nối
        public override async Task OnConnectedAsync()
        {
            // Lấy AccountId từ session
            var accountId = _httpContextAccessor.HttpContext.Session.GetString("AccountId");
            if (accountId != null)
            {
                ConnectedUsers[accountId] = Context.ConnectionId; // Lưu kết nối
                await Clients.All.SendAsync("UserConnected", accountId);
            }
            await base.OnConnectedAsync();
        }
        public async Task SendMessage(string user, string message)
        {
            await Clients.All.SendAsync("ReceiveMessage", user, message);
        }
        // Gửi tin nhắn từ admin đến người dùng

        // Phương thức này được gọi khi người dùng ngắt kết nối
        public override async Task OnDisconnectedAsync(Exception? exception)
        {
            var accountId = _httpContextAccessor.HttpContext.Session.GetString("AccountId");
            if (accountId != null)
            {
                ConnectedUsers.TryRemove(accountId, out _); // Xóa kết nối
                await Clients.All.SendAsync("UserDisconnected", accountId);
            }
            await base.OnDisconnectedAsync(exception);
        }
    }
}
