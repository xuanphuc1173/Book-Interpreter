using EXE.Hubs;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.SignalR;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace EXE.Middleware
{
    public class VisitorTrackingMiddleware
    {
        //private readonly RequestDelegate _next;
        //private readonly IHubContext<VisitorHub> _hubContext;
        //private readonly string _logFilePath = "wwwroot/visitors.log";
        //private static int _pageViews = 0;
        //private static HashSet<string> _uniqueIps = new HashSet<string>();
        //private static readonly object _lock = new object();
        //public VisitorTrackingMiddleware(RequestDelegate next, IHubContext<VisitorHub> hubContext)
        //{
        //    _next = next;
        //    _hubContext = hubContext;

        //    // Đọc file log nếu có
        //    if (File.Exists(_logFilePath))
        //    {
        //        var lines = File.ReadAllLines(_logFilePath);
        //        if (lines.Length > 0)
        //        {
        //            if (int.TryParse(lines[0], out int pageViews))
        //            {
        //                _pageViews = pageViews; // Dòng đầu chỉ là số page views
        //            }
        //            else
        //            {
        //                // Xử lý lỗi nếu không parse được số
        //                _pageViews = 0;
        //            }
        //        }

        //        _uniqueIps = new HashSet<string>(lines.Skip(1)); // Các dòng còn lại là danh sách IP
        //    }

        //}

        //public async Task InvokeAsync(HttpContext context)
        //{
        //    // Kiểm tra nếu request là GET và không phải là tài nguyên tĩnh (CSS, JS, hình ảnh, API)
        //    if (context.Request.Method == "GET")
        //    {
        //        var path = context.Request.Path.ToString().ToLower();
        //        if (!path.StartsWith("/css") && !path.StartsWith("/js") && !path.StartsWith("/images") && !path.StartsWith("/api"))
        //        {
        //            // Tăng số page views
        //            _pageViews++;

        //            // Lấy địa chỉ IP của client
        //            var ipAddress = context.Connection.RemoteIpAddress.ToString();

        //            // Nếu IP chưa tồn tại trong danh sách thì thêm vào
        //            if (!_uniqueIps.Contains(ipAddress))
        //            {
        //                _uniqueIps.Add(ipAddress);
        //            }

        //            // Lưu lại vào file log
        //            SaveLog();

        //            // Gọi phương thức gửi dữ liệu tới client
        //            await _hubContext.Clients.All.SendAsync("ReceiveVisitorData", _pageViews, _uniqueIps.Count);
        //        }
        //    }

        //    // Gọi tiếp các middleware khác
        //    await _next(context);
        //}

        //private void SaveLog()
        //{
        //    lock (_lock) // Đảm bảo chỉ một luồng có thể vào đây tại một thời điểm
        //    {
        //        // Lưu số page views và danh sách IP vào file log
        //        var logData = $"{_pageViews}\n{string.Join("\n", _uniqueIps)}";
        //        File.WriteAllText(_logFilePath, logData);
        //    }
        //}
    }
}
