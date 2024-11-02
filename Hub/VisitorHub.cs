using Microsoft.AspNetCore.SignalR;

namespace EXE.Hubs
{
    public class VisitorHub : Hub
    {
        // Phương thức này có thể được gọi từ server để gửi dữ liệu mới tới client
        public async Task SendVisitorData(int pageViews, int uniqueVisitors)
        {
            await Clients.All.SendAsync("ReceiveVisitorData", pageViews, uniqueVisitors);
        }
    }
}
