using Microsoft.AspNetCore.Mvc;
using EXE.Models.Authentication;
using EXE.Middleware;
using System.IO;
using System.Linq;
using EXE.Services;
using Microsoft.EntityFrameworkCore;
using EXE.Models;
using System.Globalization;

namespace EXE.Controllers
{
    public class AdminController : Controller
    {
        private readonly string _logFilePath = "wwwroot/visitors.log";
        private readonly IVisitorService _visitorService;
        private readonly Exe101Context _context;

        public AdminController(IVisitorService visitorService, Exe101Context context)
        {
            _visitorService = visitorService;
            _context = context;
        }

        // Action Method để trả về dữ liệu PageViews và UniqueVisitors dưới dạng JSON
        public IActionResult MonthlyStatistics()
        {
            var monthlyStats = _visitorService.GetAllMonthlyStatistics();

            // Tạo mảng để lưu trữ page views và unique visitors cho tất cả các tháng
            var pageViewsByMonth = new int[12];
            var uniqueVisitorsByMonth = new int[12];

            // Lặp qua từng tháng để lấy số liệu
            foreach (var month in monthlyStats.Keys)
            {
                try
                {
                    // Thay đổi cách phân tích tháng
                    DateTime monthDateTime = DateTime.ParseExact(month, "yyyy-MM", CultureInfo.InvariantCulture);
                    int monthIndex = monthDateTime.Month - 1; // Lấy chỉ số tháng từ DateTime
                    pageViewsByMonth[monthIndex] = monthlyStats[month].pageViews;
                    uniqueVisitorsByMonth[monthIndex] = monthlyStats[month].uniqueVisitors;
                }
                catch (FormatException ex)
                {
                    // Xử lý lỗi nếu có
                    Console.WriteLine($"Error parsing month '{month}': {ex.Message}");
                }
            }

            // Gán vào ViewBag để sử dụng trong View
            ViewBag.PageViewsByMonth = pageViewsByMonth;
            ViewBag.UniqueVisitorsByMonth = uniqueVisitorsByMonth;

            return View();
        }




        // Action Method AdminHome có Authentication và hiển thị PageViews/UniqueVisitors
        [Authentication]
        public IActionResult AdminHome()
        {
            int pageViews = 0;
            int uniqueVisitors = 0;

            // Đọc file log nếu tồn tại
            if (System.IO.File.Exists(_logFilePath))
            {
                var lines = System.IO.File.ReadAllLines(_logFilePath);

                // Lấy dòng đầu tiên và phân tích để lấy page views và unique visitors
                var totalStats = lines[0].Split(','); // Chia theo dấu phẩy

                if (totalStats.Length == 2) // Kiểm tra có đúng 2 phần không
                {
                    // Phân tích page views
                    var pageViewsPart = totalStats[0].Trim(); // "100 page views"
                    var pageViewsStr = pageViewsPart.Split(' ')[0]; // "100"
                    pageViews = int.Parse(pageViewsStr); // Chuyển đổi thành số nguyên

                    // Phân tích unique visitors
                    var uniqueVisitorsPart = totalStats[1].Trim(); // "75 unique visitors"
                    var uniqueVisitorsStr = uniqueVisitorsPart.Split(' ')[0]; // "75"
                    uniqueVisitors = int.Parse(uniqueVisitorsStr); // Chuyển đổi thành số nguyên
                }
                else
                {
                    // Xử lý lỗi nếu không có đúng 2 phần
                    pageViews = 0;
                    uniqueVisitors = 0;
                }
            }

            int totalInterpreters = GetTotalInterpreters();
            int totalCustomers = GetTotalCustomers();

            _visitorService.LogVisitor(HttpContext);

            // Lấy thống kê cho tháng hiện tại
            var monthlyStats = _visitorService.GetAllMonthlyStatistics();
            var currentMonth = DateTime.Now.ToString("MMM");

            // Lấy thông tin cho tháng hiện tại
            ViewBag.PageViewsThisMonth = monthlyStats.ContainsKey(currentMonth) ? monthlyStats[currentMonth].pageViews : 0;
            ViewBag.UniqueVisitorsThisMonth = monthlyStats.ContainsKey(currentMonth) ? monthlyStats[currentMonth].uniqueVisitors : 0;

            // Gán thống kê cho tất cả các tháng vào ViewBag
            ViewBag.PageViewsByMonth = new int[12];
            ViewBag.UniqueVisitorsByMonth = new int[12];

            foreach (var month in monthlyStats.Keys)
            {
                try
                {
                    // Phân tích tháng từ chuỗi yyyy-MM
                    DateTime monthDateTime = DateTime.ParseExact(month, "yyyy-MM", CultureInfo.InvariantCulture);
                    int monthIndex = monthDateTime.Month - 1; // Lấy chỉ số tháng từ DateTime
                    ViewBag.PageViewsByMonth[monthIndex] = monthlyStats[month].pageViews;
                    ViewBag.UniqueVisitorsByMonth[monthIndex] = monthlyStats[month].uniqueVisitors;
                }
                catch (FormatException ex)
                {
                    // Xử lý lỗi nếu có
                    Console.WriteLine($"Error parsing month '{month}': {ex.Message}");
                }
            }

            // Truyền dữ liệu vào ViewBag để hiển thị trên giao diện
            ViewBag.PageViews = pageViews;
            ViewBag.UniqueVisitors = uniqueVisitors;
            ViewBag.TotalInterpreters = totalInterpreters;
            ViewBag.TotalCustomers = totalCustomers;

            var (monthlyBookings, monthlyCosts, tenPercentCosts, ninetyPercentCosts, monthlyReviews) = GetMonthlyBookings();
            ViewBag.MonthlyBookings = monthlyBookings;
            ViewBag.MonthlyCosts = monthlyCosts;
            ViewBag.TenPercentCosts = tenPercentCosts;
            ViewBag.NinetyPercentCosts = ninetyPercentCosts;
            ViewBag.MonthlyReviews = monthlyReviews;

            return View();
        }

        public int GetTotalInterpreters()
        {
            // Đếm tổng số lượng thông dịch viên từ database
            return _context.Interpreters.Count();
        }

        public int GetTotalCustomers()
        {
            // Đếm tổng số lượng khách hàng từ database
            return _context.Account.Count();
        }



        [HttpGet]
        public JsonResult GetMonthlyData()
        {
            var (monthlyBookings, monthlyCosts, tenPercentCosts, ninetyPercentCosts, monthlyReviews) = GetMonthlyBookings();
            return Json(new
            {
                MonthlyBookings = monthlyBookings,
                MonthlyReviews = monthlyReviews,
                MonthlyCosts = monthlyCosts,
                TenPercentCosts = tenPercentCosts,
                NinetyPercentCosts = ninetyPercentCosts
            });
        }

        public List<int> GetAvailableYears()
        {
            return _context.Bookings
                .Select(b => b.CreatedDate.Year)
                .Distinct()
                .OrderByDescending(y => y)
                .ToList();
        }

        private (int[] monthlyBookings, decimal[] monthlyCosts, decimal[] tenPercentCosts, decimal[] ninetyPercentCosts, int[] monthlyReviews) GetMonthlyBookings()
        {
            var currentYear = DateTime.Now.Year;
            System.Diagnostics.Debug.WriteLine("Current Year: " + currentYear);

            var monthlyBookings = new int[12];
            var monthlyCosts = new decimal[12];
            var tenPercentCosts = new decimal[12];
            var ninetyPercentCosts = new decimal[12];
            var monthlyReviews = new int[12]; // Mảng lưu số lượng đánh giá theo tháng

            // Lấy dữ liệu booking từ cơ sở dữ liệu
            var bookings = _context.Bookings
                .Where(b => b.CreatedDate.Year == currentYear)
                .GroupBy(b => b.CreatedDate.Month)
                .Select(g => new
                {
                    Month = g.Key,
                    Count = g.Count(),
                    TotalCost = g.Sum(b => b.TotalCost) // Thay đổi Cost với tên thuộc tính thực tế của bạn
                })
                .ToList();

            // Lấy dữ liệu reviews từ cơ sở dữ liệu
            var reviews = _context.Reviews
                .Where(r => r.ReviewDate.HasValue && r.ReviewDate.Value.Year == currentYear) // Kiểm tra HasValue trước khi lấy Year
                .GroupBy(r => r.ReviewDate.Value.Month) // Sử dụng Value để truy cập vào Month
                .Select(g => new
                {
                    Month = g.Key,
                    Count = g.Count()
                })
                .ToList();


            // Gán tổng booking, tổng chi phí cho từng tháng
            foreach (var booking in bookings)
            {
                monthlyBookings[booking.Month - 1] = booking.Count;
                monthlyCosts[booking.Month - 1] = booking.TotalCost * 1000;
                tenPercentCosts[booking.Month - 1] = booking.TotalCost * 0.15m *1000;
                ninetyPercentCosts[booking.Month - 1] = booking.TotalCost * 0.85m * 1000;
            }

            // Gán tổng số lượng review cho từng tháng
            foreach (var review in reviews)
            {
                monthlyReviews[review.Month - 1] = review.Count;
            }

            // Kiểm tra dữ liệu đã gán vào mảng
            System.Diagnostics.Debug.WriteLine("Monthly Reviews Array: " + string.Join(", ", monthlyReviews));

            return (monthlyBookings, monthlyCosts, tenPercentCosts, ninetyPercentCosts, monthlyReviews);
        }





        // Các Action Method khác
        public IActionResult Chatbox()
        {
            return View();
        }

        public IActionResult Table()
        {
            return View();
        }

        public IActionResult VirtualReality()
        {
            return View();
        }

        public IActionResult Billing()
        {
            return View();
        }

        public IActionResult Notification()
        {
            return View();
        }

        public IActionResult RTL()
        {
            return View();
        }
    }
}
