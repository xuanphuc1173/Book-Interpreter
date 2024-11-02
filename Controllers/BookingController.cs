using EXE.Helpers;
using EXE.Models;
using EXE.Models.Authentication;
using EXE.Services;
using EXE.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Identity.Client;
using Newtonsoft.Json;
using System.Net.Http;
using System.Security.Principal;
using YourProjectNamespace.Services;

namespace EXE.Controllers
{
    public class BookingController : Controller
    {
        private readonly Exe101Context _context;
        private readonly IVnPayService _vnPayService;
        private readonly ILogger<BookingController> _logger;
        private readonly EmailService _emailService;
        private readonly PaypalClient _paypalClient;

        public BookingController(Exe101Context context, IVnPayService vnPayService, ILogger<BookingController> logger, EmailService emailService, PaypalClient paypalClient)
        {
            _logger = logger;
            _context = context;
            _vnPayService = vnPayService;
            _emailService = emailService;
            _paypalClient = paypalClient;
        }
        [HttpGet]
        public IActionResult CreateBooking(int interpreterId, DateTime date)
        {

            var interpreter = _context.Interpreters.Find(interpreterId);
            if (interpreter == null)
            {
                return NotFound("Interpreter not found.");
            }

            var accountId = HttpContext.Session.GetInt32("AccountId");
            if (accountId != null)
            {
                var points = _context.Points.FirstOrDefault(p => p.AccountId == accountId);
                ViewBag.TotalPoints = points?.TotalPoints ?? 0;
                ViewBag.InvitationCode = points?.InvitationCode;
            }

            var startTimeJson = HttpContext.Session.GetString("StartTime");
            var endTimeJson = HttpContext.Session.GetString("EndTime");

            TimeOnly startTime = TimeOnly.MinValue; 
            TimeOnly endTime = TimeOnly.MinValue;   

            if (!string.IsNullOrEmpty(startTimeJson))
            {
                startTime = TimeOnly.Parse(startTimeJson); 
            }

            if (!string.IsNullOrEmpty(endTimeJson))
            {
                endTime = TimeOnly.Parse(endTimeJson); 
            }

            var selectedDateString = HttpContext.Session.GetString("SelectedDate");
            DateOnly selectedDate;
            if (DateOnly.TryParse(selectedDateString, out selectedDate))
            {
                var model = new CreateBookingViewModel
                {
                    InterpreterId = interpreterId,
                    AccountId = accountId.Value,
                    StartTime = selectedDate.ToDateTime(startTime), 
                    EndTime = selectedDate.ToDateTime(endTime)      
                };
                ViewBag.PaypalClientId = _paypalClient.ClientId;
                return View("~/Views/User/Booking.cshtml", model);
            }
            ViewBag.PaypalClientId = _paypalClient.ClientId;
            _logger.LogInformation($"Paypal Client ID: {_paypalClient?.ClientId}");

            return View("~/Views/User/Booking.cshtml");
        }


        [HttpPost]
        public IActionResult CreateBooking(int interpreterId, CreateBookingViewModel model)
        {
            var accountId = HttpContext.Session.GetInt32("AccountId");
            if (accountId == null)
            {
                return Unauthorized("User not logged in.");
            }

            var interpreter = _context.Interpreters
                .FirstOrDefault(i => i.InterpreterId == interpreterId);

            if (interpreter == null)
            {
                ModelState.AddModelError("", "Interpreter not found.");
                return View("~/Views/User/Booking.cshtml", model);
            }

            var points = _context.Points.FirstOrDefault(p => p.AccountId == accountId);
            int availablePoints = points?.TotalPoints ?? 0;
            int totalPoints = model.TotalPoints;

            if (totalPoints < 0)
            {
                ModelState.AddModelError("totalPoints", "Points cannot be negative.");
                model.InterpreterId = interpreterId;
                return View("~/Views/User/Booking.cshtml", model);
            }

            if (totalPoints > 0 && totalPoints % 100 != 0)
            {
                ModelState.AddModelError("totalPoints", "You must enter points as a multiple of 100.");
                model.InterpreterId = interpreterId;
                return View("~/Views/User/Booking.cshtml", model);
            }

            if (totalPoints > 0 && totalPoints < 100)
            {
                ModelState.AddModelError("totalPoints", "You need to enter at least 100 points for a discount.");
                model.InterpreterId = interpreterId;
                return View("~/Views/User/Booking.cshtml", model);
            }

            if (totalPoints > availablePoints)
            {
                ModelState.AddModelError("totalPoints", "You don't have enough points.");
                model.InterpreterId = interpreterId;
                return View("~/Views/User/Booking.cshtml", model);
            }



            // Tính toán thời gian bắt đầu và kết thúc từ model
            var startTime = model.StartTime; // Lưu ý bạn cần có StartTime và EndTime trong CreateBookingViewModel
            var endTime = model.EndTime;

            // Kiểm tra thời gian
            if (startTime >= endTime)
            {
                ModelState.AddModelError("", "End time must be after start time.");
                return View("~/Views/User/Booking.cshtml", model);
            }

            // Tính toán tổng chi phí
            var totalHours = (endTime - startTime).TotalHours;
            var totalCost = (decimal)totalHours * interpreter.HourlyRate;

            // Áp dụng giảm giá nếu có đủ điểm
            if (totalPoints >= 100)
            {
                var discountPercentage = (totalPoints / 100) * 10; // Mỗi 100 điểm giảm 10%
                discountPercentage = Math.Min(discountPercentage, 100); // Giới hạn tối đa giảm 100%

                // Áp dụng giảm giá vào tổng chi phí
                totalCost = totalCost - (totalCost * discountPercentage / 100);
            }

            var booking = new Bookings
            {
                InterpreterId = interpreterId,
                AccountId = accountId.Value,
                StartTime = startTime,
                EndTime = endTime,
                CreatedDate = DateTime.Now,
                TotalCost = totalCost,
                Location = model.Location
            };

            // Lưu thông tin booking vào session và thông tin điểm đã sử dụng
            HttpContext.Session.SetString("BookingInfo", JsonConvert.SerializeObject(booking));
            HttpContext.Session.SetInt32("UsedPoints", totalPoints);

            var paymentRequestModel = new VnPaymentRequestModel
            {
                OrderId = new Random().Next(1000, 10000),
                Description = $"{model.Location}",
                Amount = (int)(totalCost * 1000),
                CreatedDate = DateTime.Now
            };

            return Redirect(_vnPayService.CreatePaymentUrl(HttpContext, paymentRequestModel));
        }




        #region Paypal payment
        // [Authorize]
        [HttpPost("/Booking/create-paypal-order")]
        public async Task<IActionResult> CreatePaypalOrder([FromBody] CreateBookingViewModel model)
        {
            if (model == null)
            {
                return BadRequest("Model cannot be null.");
            }
            Console.WriteLine($"InterpreterId: {model.InterpreterId}, StartTime: {model.StartTime}, EndTime: {model.EndTime}, TotalPoints: {model.TotalPoints}, Location:{model.Location}");

            var accountId = HttpContext.Session.GetInt32("AccountId");
            if (accountId == null)
            {
                return Unauthorized("User not logged in.");
            }

            var interpreter = await _context.Interpreters.AsNoTracking()
                   .FirstOrDefaultAsync(i => i.InterpreterId == model.InterpreterId);

            if (interpreter == null)
            {
                return BadRequest("Interpreter not found.");
            }

            if (model.StartTime == DateTime.MinValue || model.EndTime == DateTime.MinValue)
            {
                return BadRequest("Start time or end time is invalid.");
            }

            if (model.StartTime >= model.EndTime)
            {
                return BadRequest("Start time must be earlier than end time.");
            }

            if (string.IsNullOrEmpty(model.Location))
            {
                return BadRequest("Location is required.");
            }

            int totalPoints = model.TotalPoints;

            var totalHours = (model.EndTime - model.StartTime).TotalHours;
            var totalCost = (decimal)totalHours * interpreter.HourlyRate;

            if (totalPoints >= 100)
            {
                var discountPercentage = (totalPoints / 100) * 10;
                discountPercentage = Math.Min(discountPercentage, 100);
                totalCost -= totalCost * discountPercentage / 100;
            }
            var exchangeRate = 24000;
            var totalCostUSD = Math.Round(totalCost / exchangeRate * 1000, 2);
            var booking = new Bookings
            {
                InterpreterId = model.InterpreterId,
                AccountId = accountId.Value,
                StartTime = model.StartTime,
                EndTime = model.EndTime,
                CreatedDate = DateTime.Now,
                TotalCost = totalCost * 1000,
                Location = model.Location
            };

            HttpContext.Session.SetString("BookingInfo", JsonConvert.SerializeObject(booking));
            HttpContext.Session.SetInt32("UsedPoints", totalPoints);

            try
            {
                var response = await _paypalClient.CreateOrder(totalCostUSD, "USD", "DH" + DateTime.Now.Ticks.ToString());
                return Ok(response); 
            }
            catch (Exception ex)
            {
                return BadRequest(new { error = ex.Message, stackTrace = ex.StackTrace });
            }
        }




        [HttpPost("/Booking/capture-paypal-order")]
        public async Task<IActionResult> CapturePaypalOrder(string orderID, CancellationToken cancellationToken)
        {
            try
            {
                var response = await _paypalClient.CaptureOrder(orderID);

                // Lấy thông tin booking từ session
                var bookingJson = HttpContext.Session.GetString("BookingInfo");
                var booking = JsonConvert.DeserializeObject<Bookings>(bookingJson);

                if (booking == null)
                {
                    TempData["Message"] = "Thông tin đơn hàng không hợp lệ.";
                    return View("~/Views/User/Searchinterpreter.cshtml");
                }

                var accountId = booking.AccountId;
                var points = _context.Points.FirstOrDefault(p => p.AccountId == accountId);

                if (points != null)
                {
                    // Lấy số điểm đã dùng từ session và trừ điểm tương ứng
                    var usedPoints = HttpContext.Session.GetInt32("UsedPoints");
                    if (usedPoints.HasValue)
                    {
                        points.TotalPoints -= usedPoints.Value;
                    }

                    // Cập nhật điểm trong cơ sở dữ liệu
                    _context.Points.Update(points);
                }

                // Lưu booking vào cơ sở dữ liệu
                _context.Bookings.Add(booking);
                await _context.SaveChangesAsync();

                // Xóa thông tin booking và điểm đã sử dụng khỏi session
                HttpContext.Session.Remove("BookingInfo");
                HttpContext.Session.Remove("UsedPoints");
                return Ok(response);
            }
            catch (Exception ex)
            {
                var error = new { ex.GetBaseException().Message };
                return BadRequest(error);
            }
        }

        #endregion

        public async Task<IActionResult> PaymentCallBack()
        {
            var response = _vnPayService.PaymentExecute(Request.Query);
            var bookingJson = HttpContext.Session.GetString("BookingInfo");

            if (response == null || response.VnPayResponseCode != "00")
            {
                TempData["Message"] = $"Lỗi thanh toán VN Pay: {response?.VnPayResponseCode}";
                return View("~/Views/Booking/PaymentFail.cshtml");
            }

            var booking = JsonConvert.DeserializeObject<Bookings>(bookingJson);
            if (booking == null)
            {
                TempData["Message"] = "Thông tin đơn hàng không hợp lệ.";
                return View("~/Views/User/Searchinterpreter.cshtml");
            }

            var accountId = booking.AccountId;
            var points = _context.Points.FirstOrDefault(p => p.AccountId == accountId);

            if (points != null)
            {
                // Lấy số điểm đã dùng từ session
                var usedPoints = HttpContext.Session.GetInt32("UsedPoints");
                if (usedPoints.HasValue)
                {
                    points.TotalPoints -= usedPoints.Value; // Trừ điểm tương ứng
                }

                // Cập nhật điểm trong cơ sở dữ liệu
                _context.Points.Update(points);
            }

            _context.Bookings.Add(booking);
            await _context.SaveChangesAsync();
            HttpContext.Session.Remove("BookingInfo");
            HttpContext.Session.Remove("UsedPoints");
            TempData["Message"] = "Thanh toán VNPay thành công";
            return View("~/Views/Booking/PaymentSuccess.cshtml");
        }
        public IActionResult PaymentSuccess()
        {
            return View("~/Views/Booking/PaymentSuccess.cshtml");
        }
        //-------------------------------------------------------------------------
        [Authentication]

        public async Task<IActionResult> Index()
        {
            var bookings = await _context.Bookings
                .Include(b => b.Account)
                .Include(b => b.Interpreter)
                .ToListAsync();

            foreach (var booking in bookings)
            {
                // Nhân TotalCost với 1000
                booking.TotalCost *= 1000;
            }
            return View("~/Views/Admin/BookingManage/Index.cshtml", bookings);
        }
        [Authentication]

        // GET: Bookings/Details/5
        public async Task<IActionResult> Details(int id)
        {
            var booking = await _context.Bookings
                .Include(b => b.Account)
                .Include(b => b.Interpreter)
                .FirstOrDefaultAsync(m => m.BookingId == id);
            if (booking == null)
            {
                return NotFound();
            }
            return View("~/Views/Admin/BookingManage/Details.cshtml", booking);
        }

        // GET: Bookings/Create
        public IActionResult Create()
        {
            return View();
        }
        [Authentication]

        // POST: Bookings/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("BookingId,AccountId,InterpreterId,StartTime,EndTime,TotalCost,Location")] Bookings booking)
        {
            if (ModelState.IsValid)
            {
                _context.Add(booking);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            return View(booking);
        }
        [Authentication]

        // GET: Bookings/Edit/5
        public async Task<IActionResult> Edit(int id)
        {
            var booking = await _context.Bookings.FindAsync(id);
            if (booking == null)
            {
                return NotFound();
            }
            return View(booking);
        }
        [Authentication]

        // POST: Bookings/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("BookingId,AccountId,InterpreterId,StartTime,EndTime,TotalCost,Location")] Bookings booking)
        {
            if (id != booking.BookingId)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(booking);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!BookingExists(booking.BookingId))
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }
                return RedirectToAction(nameof(Index));
            }
            return View(booking);
        }
        [Authentication]

        // GET: Bookings/Delete/5
        public async Task<IActionResult> Delete(int id)
        {
            var booking = await _context.Bookings
                .Include(b => b.Account)
                .Include(b => b.Interpreter)
                .FirstOrDefaultAsync(m => m.BookingId == id);
            if (booking == null)
            {
                return NotFound();
            }
            return View(booking);
        }
        [Authentication]

        // POST: Bookings/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var booking = await _context.Bookings.FindAsync(id);
            _context.Bookings.Remove(booking);
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool BookingExists(int id)
        {
            return _context.Bookings.Any(e => e.BookingId == id);
        }
        [Authentication]

        [HttpGet]
        public async Task<IActionResult> BookingsByAccountId(int accountId)
        {
            Console.WriteLine("AccountId being searched: " + accountId);
            var bookings = await _context.Bookings
                .Where(b => b.AccountId == accountId)
                .Include(b => b.Interpreter)
                .Include(b => b.Account)
                .ToListAsync();

            foreach (var booking in bookings)
            {
                booking.TotalCost *= 1000;
                booking.HasReview = await _context.Reviews
                    .AnyAsync(r => r.InterpreterId == booking.InterpreterId && r.AccountId == accountId);
            }

            if (bookings == null || !bookings.Any())
            {
                bookings = new List<Bookings>();
            }

            return View("~/Views/User/Appointment.cshtml", bookings);
        }

    }

}


