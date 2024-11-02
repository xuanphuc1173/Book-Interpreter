using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace EXE.Services
{
    public interface IVisitorService
    {
        int GetTotalPageViews(); // Tổng số page views
        int GetTotalUniqueVisitors(); // Tổng số unique visitors
        int GetPageViewsForCurrentMonth(); // Page views trong tháng hiện tại
        int GetUniqueVisitorsForCurrentMonth(); // Unique visitors trong tháng hiện tại
        Dictionary<string, (int pageViews, int uniqueVisitors)> GetAllMonthlyStatistics(); // Thống kê theo tháng
        void LogVisitor(HttpContext context); // Ghi nhận visitor
    }

    public class VisitorService : IVisitorService
    {
        private readonly string _logFilePath = "wwwroot/visitors.log";
        private Dictionary<string, (int pageViews, int uniqueVisitors)> _monthlyStatistics; // Lưu thống kê theo từng tháng
        private HashSet<string> _currentMonthUniqueIps; // Lưu IP unique cho tháng hiện tại
        private int _totalPageViews; // Tổng số page views
        private int _totalUniqueVisitors; // Tổng số unique visitors
        private const string VisitorSessionKey = "VisitorLogged";

        public VisitorService()
        {
            _monthlyStatistics = new Dictionary<string, (int, int)>();
            _currentMonthUniqueIps = new HashSet<string>();
            LoadVisitorData();
        }

        // Load dữ liệu từ file log
        private void LoadVisitorData()
        {
            if (File.Exists(_logFilePath))
            {
                var lines = File.ReadAllLines(_logFilePath);

                // Kiểm tra xem file có ít nhất một dòng
                if (lines.Length > 0)
                {
                    var totalStats = lines[0].Split(',');
                    if (totalStats.Length == 2) // Kiểm tra nếu có đúng 2 phần
                    {
                        _totalPageViews = int.Parse(totalStats[0].Trim().Split(' ')[0]);
                        _totalUniqueVisitors = int.Parse(totalStats[1].Trim().Split(' ')[0]);
                    }
                    else
                    {
                        // Xử lý lỗi nếu dữ liệu không đúng định dạng
                        _totalPageViews = 0;
                        _totalUniqueVisitors = 0;
                    }

                    // Các dòng còn lại là thống kê theo tháng
                    foreach (var line in lines.Skip(1))
                    {
                        var parts = line.Split(':');
                        if (parts.Length == 2)
                        {
                            var month = parts[0].Trim();
                            var stats = parts[1].Split(',');

                            if (stats.Length == 2) // Kiểm tra có đúng 2 phần trong thống kê
                            {
                                var pageViews = int.Parse(stats[0].Trim().Split(' ')[0]);
                                var uniqueVisitors = int.Parse(stats[1].Trim().Split(' ')[0]);

                                _monthlyStatistics[month] = (pageViews, uniqueVisitors);
                            }
                        }
                    }
                }
            }

            // Khởi tạo cho tháng hiện tại nếu chưa có
            var currentMonth = DateTime.Now.ToString("yyyy-MM");
            if (!_monthlyStatistics.ContainsKey(currentMonth))
            {
                _monthlyStatistics[currentMonth] = (0, 0);
            }
        }


        // Tổng số page views
        public int GetTotalPageViews() => _totalPageViews;

        // Tổng số unique visitors
        public int GetTotalUniqueVisitors() => _totalUniqueVisitors;

        // Số page views trong tháng hiện tại
        public int GetPageViewsForCurrentMonth()
        {
            var currentMonth = DateTime.Now.ToString("yyyy-MM");
            return _monthlyStatistics[currentMonth].pageViews;
        }

        // Số unique visitors trong tháng hiện tại
        public int GetUniqueVisitorsForCurrentMonth()
        {
            var currentMonth = DateTime.Now.ToString("yyyy-MM");
            return _monthlyStatistics[currentMonth].uniqueVisitors;
        }

        // Thống kê theo từng tháng
        public Dictionary<string, (int pageViews, int uniqueVisitors)> GetAllMonthlyStatistics()
        {
            return _monthlyStatistics;
        }

        // Ghi nhận visitor và page views
        public void LogVisitor(HttpContext context)
        {
            // Lấy tag phiên từ session
            string visitorSessionTag = context.Session.GetString(VisitorSessionKey);
            string currentMonth = DateTime.Now.ToString("yyyy-MM");

            // Kiểm tra nếu visitor đã được ghi nhận trong session
            if (string.IsNullOrEmpty(visitorSessionTag))
            {
                // Tạo tag mới cho visitor này
                visitorSessionTag = Guid.NewGuid().ToString(); // Tạo một tag duy nhất
                context.Session.SetString(VisitorSessionKey, visitorSessionTag);

                // Tăng số unique visitors cho tháng hiện tại
                _monthlyStatistics[currentMonth] = (
                    _monthlyStatistics[currentMonth].pageViews,
                    _monthlyStatistics[currentMonth].uniqueVisitors + 1
                );

                // Tăng tổng số unique visitors
                _totalUniqueVisitors++;
            }

            // Mỗi lần người dùng mở một tab mới, sẽ được coi là một lần truy cập mới
            // Tăng số page view cho tháng hiện tại
            _monthlyStatistics[currentMonth] = (
                _monthlyStatistics[currentMonth].pageViews + 1,
                _monthlyStatistics[currentMonth].uniqueVisitors
            );

            // Tăng tổng số page views
            _totalPageViews++;

            // Lưu dữ liệu mỗi khi có thay đổi
            SaveVisitorData();
        }


        // Lưu dữ liệu vào file log
        private void SaveVisitorData()
        {
            var lines = new List<string>
            {
                $"{_totalPageViews} page views, {_totalUniqueVisitors} unique visitors" // Dòng đầu chứa tổng
            };

            // Ghi dữ liệu từng tháng
            lines.AddRange(_monthlyStatistics.Select(kv =>
                $"{kv.Key}: {kv.Value.pageViews} page views, {kv.Value.uniqueVisitors} unique visitors"
            ));

            File.WriteAllLines(_logFilePath, lines);
        }
    }
}
