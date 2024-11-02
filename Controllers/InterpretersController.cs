using EXE.Models;
using EXE.Models.Authentication;
using EXE.Services;
using EXE.ViewModels;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using System.Drawing;
using System.Linq;

namespace EXE.Controllers
{
    public class InterpretersController : Controller
    {
        private readonly Exe101Context _context;
        private readonly IWebHostEnvironment _webHostEnvironment;
        private readonly PaypalClient _paypalClient;

        public InterpretersController(Exe101Context context, IWebHostEnvironment webHostEnvironment, PaypalClient paypalClient)
        {
            _context = context;
            _webHostEnvironment = webHostEnvironment;
            _paypalClient = paypalClient;

        }
        [HttpGet]
        public IActionResult SearchAvailability()
        {

            return View();
        }

        [HttpPost]
        public IActionResult SearchAvailability(DateOnly date, TimeOnly startTime, TimeOnly endTime, string interpreterType, string location, string language)
        {
            // Lấy tất cả các booking đã có trong ngày đã chọn
            var bookedSlots = _context.Bookings
                .Where(b => b.StartTime.Date == date.ToDateTime(TimeOnly.MinValue).Date &&
                            b.EndTime.Date == date.ToDateTime(TimeOnly.MinValue).Date)
                .Select(b => new
                {
                    b.InterpreterId,
                    b.StartTime,
                    b.EndTime
                })
                .ToList();

            // Lấy tất cả lịch khả dụng cho ngày đã chọn
            var availableSlots = _context.CalendarAvailability
                .Where(a => a.AvailabilityDate == date)
                .Select(a => new AvailabilityViewModel
                {
                    InterpreterId = a.InterpreterId,
                    StartTime = a.StartTime,
                    EndTime = a.EndTime,
                    Interpreter = _context.Interpreters.FirstOrDefault(i => i.InterpreterId == a.InterpreterId)
                })
                .ToList();

            // Lọc lịch khả dụng không bị trùng với booking và đảm bảo khung giờ nằm trong khung thời gian khả dụng
            var availableFilteredSlots = availableSlots
                .Where(a =>
                    !bookedSlots.Any(b =>
                        b.InterpreterId == a.InterpreterId &&
                        a.StartTime.ToTimeSpan() < b.EndTime.TimeOfDay &&
                        a.EndTime.ToTimeSpan() > b.StartTime.TimeOfDay) &&
                    a.StartTime.ToTimeSpan() <= startTime.ToTimeSpan() &&  // Khung giờ bắt đầu nằm trong khoảng thời gian khả dụng
                    a.EndTime.ToTimeSpan() >= endTime.ToTimeSpan()        // Khung giờ kết thúc nằm trong khoảng thời gian khả dụng
                )
                .ToList();

            // Lọc lại theo thời gian đã chọn
            var filteredSlots = availableFilteredSlots
                .Where(a =>
                    (a.StartTime.ToTimeSpan() >= startTime.ToTimeSpan() && a.StartTime.ToTimeSpan() < endTime.ToTimeSpan()) ||
                    (a.EndTime.ToTimeSpan() > startTime.ToTimeSpan() && a.EndTime.ToTimeSpan() <= endTime.ToTimeSpan()) ||
                    (a.StartTime.ToTimeSpan() <= startTime.ToTimeSpan() && a.EndTime.ToTimeSpan() >= endTime.ToTimeSpan())
                )
                .ToList();
            if (!string.IsNullOrEmpty(interpreterType))
            {
                Console.WriteLine($"Filtering by Interpreter Type: '{interpreterType}'");
                filteredSlots = filteredSlots
                    .Where(a => a.Interpreter.Type != null &&
                                a.Interpreter.Type.Trim().Equals(interpreterType.Trim(), StringComparison.OrdinalIgnoreCase))
                    .ToList();
            }

            if (!string.IsNullOrEmpty(location))
            {
                Console.WriteLine($"Filtering by Location: {location}");
                filteredSlots = filteredSlots
                    .Where(a => a.Interpreter.Location != null && a.Interpreter.Location.Equals(location, StringComparison.OrdinalIgnoreCase))
                    .ToList();
            }

            if (!string.IsNullOrEmpty(language))
            {
                Console.WriteLine($"Filtering by Language: '{language}'");
                filteredSlots = filteredSlots
                    .Where(a => a.Interpreter.Language != null &&
                                a.Interpreter.Language.Trim().Equals(language.Trim(), StringComparison.OrdinalIgnoreCase))
                    .ToList();
            }

            HttpContext.Session.SetString("StartTime", startTime.ToString());
            HttpContext.Session.SetString("EndTime", endTime.ToString());
            HttpContext.Session.SetString("SelectedDate", date.ToString("yyyy-MM-dd"));


            ViewBag.Date = date.ToString("yyyy-MM-dd");
            ViewBag.StartTime = startTime.ToTimeSpan();
            ViewBag.EndTime = endTime.ToTimeSpan();
            return RedirectToAction("ShowSearchResults", new { date, startTime, endTime, interpreterType, location, language });
        }

        [HttpGet]
        public IActionResult ShowSearchResults(DateOnly date, TimeOnly startTime, TimeOnly endTime, string interpreterType, string location, string language)
        {
            // Tương tự, bạn có thể sử dụng lại logic từ POST hoặc lưu trữ filteredSlots vào Session
            var bookedSlots = _context.Bookings
                .Where(b => b.StartTime.Date == date.ToDateTime(TimeOnly.MinValue).Date &&
                            b.EndTime.Date == date.ToDateTime(TimeOnly.MinValue).Date)
                .Select(b => new
                {
                    b.InterpreterId,
                    b.StartTime,
                    b.EndTime
                })
                .ToList();

            var availableSlots = _context.CalendarAvailability
                .Where(a => a.AvailabilityDate == date)
                .Select(a => new AvailabilityViewModel
                {
                    InterpreterId = a.InterpreterId,
                    StartTime = a.StartTime,
                    EndTime = a.EndTime,
                    Interpreter = _context.Interpreters.FirstOrDefault(i => i.InterpreterId == a.InterpreterId)
                })
                .ToList();

            var availableFilteredSlots = availableSlots
                .Where(a =>
                    !bookedSlots.Any(b =>
                        b.InterpreterId == a.InterpreterId &&
                        a.StartTime.ToTimeSpan() < b.EndTime.TimeOfDay &&
                        a.EndTime.ToTimeSpan() > b.StartTime.TimeOfDay) &&
                    a.StartTime.ToTimeSpan() <= startTime.ToTimeSpan() &&
                    a.EndTime.ToTimeSpan() >= endTime.ToTimeSpan()
                )
                .ToList();

            var filteredSlots = availableFilteredSlots
                .Where(a =>
                    (a.StartTime.ToTimeSpan() >= startTime.ToTimeSpan() && a.StartTime.ToTimeSpan() < endTime.ToTimeSpan()) ||
                    (a.EndTime.ToTimeSpan() > startTime.ToTimeSpan() && a.EndTime.ToTimeSpan() <= endTime.ToTimeSpan()) ||
                    (a.StartTime.ToTimeSpan() <= startTime.ToTimeSpan() && a.EndTime.ToTimeSpan() >= endTime.ToTimeSpan())
                )
                .ToList();

            // Lọc theo loại thông dịch viên
            if (!string.IsNullOrEmpty(interpreterType))
            {
                filteredSlots = filteredSlots
                    .Where(a => a.Interpreter.Type != null && a.Interpreter.Type.Equals(interpreterType, StringComparison.OrdinalIgnoreCase))
                    .ToList();
            }

            // Lọc theo địa điểm
            if (!string.IsNullOrEmpty(location))
            {
                filteredSlots = filteredSlots
                    .Where(a => a.Interpreter.Location != null && a.Interpreter.Location.Equals(location, StringComparison.OrdinalIgnoreCase))
                    .ToList();
            }

            // Lọc theo ngôn ngữ

            if (!string.IsNullOrEmpty(language))
            {
                Console.WriteLine($"Filtering by Language: '{language}'");
                filteredSlots = filteredSlots
                    .Where(a => a.Interpreter.Language != null &&
                                a.Interpreter.Language.Trim().Equals(language.Trim(), StringComparison.OrdinalIgnoreCase))
                    .ToList();
            }

            HttpContext.Session.SetString("StartTime", startTime.ToString());
            HttpContext.Session.SetString("EndTime", endTime.ToString());
            HttpContext.Session.SetString("SelectedDate", date.ToString("yyyy-MM-dd"));


            ViewBag.Date = date.ToString("yyyy-MM-dd");
            ViewBag.StartTime = startTime.ToTimeSpan();
            ViewBag.EndTime = endTime.ToTimeSpan();
            return View(filteredSlots); // Trả về view với danh sách filteredSlots
        }

        [HttpGet]
        public IActionResult Calendar()
        {
            return View();
        }

        [HttpPost]
        public IActionResult SaveAvailability(DateTime selectedDate, TimeSpan startTime, TimeSpan endTime)
        {
            var currentInterpreterId = HttpContext.Session.GetInt32("InterpreterId");

            // Chuyển đổi DateTime và TimeSpan sang DateOnly và TimeOnly
            var availability = new CalendarAvailability
            {
                InterpreterId = currentInterpreterId.Value,
                AvailabilityDate = DateOnly.FromDateTime(selectedDate),
                StartTime = TimeOnly.FromTimeSpan(startTime),
                EndTime = TimeOnly.FromTimeSpan(endTime),
                Status = "Available"
            };

            _context.CalendarAvailability.Add(availability);
            _context.SaveChanges();

            return Json(new { success = true });
        }
        [HttpPost]
        public IActionResult EditAvailability(int id , TimeSpan startTime, TimeSpan endTime)
        {
            var availability = _context.CalendarAvailability.Find(id);
            if (availability == null)
            {
                return Json(new { success = false, message = "Availability not found." });
            }

            availability.StartTime = TimeOnly.FromTimeSpan(startTime);
            availability.EndTime = TimeOnly.FromTimeSpan(endTime);
            _context.SaveChanges();

            return Json(new { success = true });
        }
        [HttpPost]
        public IActionResult DeleteAvailability(int id)
        {
            var availability = _context.CalendarAvailability.Find(id);
            if (availability == null)
            {
                return Json(new { success = false, message = "Availability not found." });
            }

            _context.CalendarAvailability.Remove(availability);
            _context.SaveChanges();
            Console.WriteLine(JsonConvert.SerializeObject(availability));
            return Json(new { success = true });
        }

        public IActionResult GetAvailability()
        {
            var currentInterpreterId = HttpContext.Session.GetInt32("InterpreterId");

            var availability = _context.CalendarAvailability
                                      .Where(a => a.InterpreterId == currentInterpreterId)
                                      .Select(a => new {
                                          id = a.AvailabilityId,
                                          start = a.AvailabilityDate.ToDateTime(a.StartTime), // Kết hợp ngày và giờ bắt đầu
                                          end = a.AvailabilityDate.ToDateTime(a.EndTime), // Kết hợp ngày và giờ kết thúc
                                          title = "Available",
                                          backgroundColor = "green"
                                      }).ToList();
            Console.WriteLine(JsonConvert.SerializeObject(availability));
            return Json(availability);
        }

        [Authentication]
		[HttpGet]
        public IActionResult Search()
        {
            var interpreter = _context.Interpreters.ToList();
            return View("~/Views/User/Searchinterpreter.cshtml", interpreter);
		}
		[Authentication]
        [HttpPost]
        public IActionResult Search(SearchViewModel searchModel)
        {
            var query = _context.Interpreters.AsQueryable();

            if (searchModel.MinHourlyRate.HasValue)
            {
                query = query.Where(i => i.HourlyRate >= searchModel.MinHourlyRate.Value);
            }

            if (searchModel.MaxHourlyRate.HasValue)
            {
                query = query.Where(i => i.HourlyRate <= searchModel.MaxHourlyRate.Value);
            }

            if (searchModel.MinExperience.HasValue)
            {
                query = query.Where(i => i.Experience >= searchModel.MinExperience.Value);
            }

            if (searchModel.MaxExperience.HasValue)
            {
                query = query.Where(i => i.Experience <= searchModel.MaxExperience.Value);
            }
            if (!string.IsNullOrEmpty(searchModel.FullName))
            {
                query = query.Where(i => i.FullName.Contains(searchModel.FullName));
            }
            if (!string.IsNullOrEmpty(searchModel.Location))
            {
                query = query.Where(i => i.Location.Contains(searchModel.Location));
            }            
            if (!string.IsNullOrEmpty(searchModel.Type))
            {
                query = query.Where(i => i.Type.Contains(searchModel.Type));
            }
            if (!string.IsNullOrEmpty(searchModel.Language))
            {
                query = query.Where(i => i.Language.Contains(searchModel.Language)); 
            }
            query = query.Where(i => i.Availability == true);
            var results = query.ToList();

            return View("~/Views/User/Searchinterpreter.cshtml", results);
        }

        [Authentication]

        [HttpGet]
        public async Task<IActionResult> DetailsInterpreter(int id)
        {
            var interpreter = await _context.Interpreters
            .Include(i => i.Reviews)
            .FirstOrDefaultAsync(i => i.InterpreterId == id);

            var reviews = await _context.Reviews
            .Include(r => r.Account)
            .ToListAsync();

            if (interpreter == null)
            {
                return NotFound();
            }
            ViewBag.PaypalClientdId = _paypalClient.ClientId;
            return View("~/Views/Interpreters/DetailsInterpreter.cshtml", interpreter);
        }

        [Authentication]

        [HttpGet]
        public async Task<IActionResult> DetailsHome(int id)
        {
            var interpreter = await _context.Interpreters
            .Include(i => i.Reviews) 
            .FirstOrDefaultAsync(i => i.InterpreterId == id);

            var reviews = await _context.Reviews
            .Include(r => r.Account) 
            .ToListAsync();

            if (interpreter == null)
            {
                return NotFound();
            }

            return View("~/Views/User/Detailinterpreter.cshtml", interpreter);
        }
        // GET: Interpreters
        [Authentication]

        public IActionResult Index()
        {
            var interpreters = _context.Interpreters.ToList();
            return View("~/Views/Admin/InterpreterManage/Index.cshtml", interpreters);
        }

        // GET: Interpreters/Details/5
        public IActionResult Details(int id)
        {
            var interpreter = _context.Interpreters.FirstOrDefault(i => i.InterpreterId == id);
            if (interpreter == null)
            {
                return NotFound();
            }
            return View("~/Views/Admin/InterpreterManage/Details.cshtml", interpreter);
        }

        // GET: Interpreters/Create
        [Authentication]

        [HttpGet]
        public IActionResult Create()
        {

            ViewBag.Languages = new SelectList(new List<string> { "English", "Vietnamese", "Chinese", "Japanese", "Korean" });
            ViewBag.Types = new SelectList(new List<string> { "Simultaneous Interpreting", "Travel", "Relay Interpreting", "Whispering Interpreting" });
            ViewBag.Locations = new SelectList(new List<string> { "Da Nang  ", "Hoi An ", "Quang Nam ", "Hue ","Quang Tri" });
            return View("~/Views/Admin/InterpreterManage/Create.cshtml");
        }

        // POST: Interpreters/Create
        [Authentication]

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(CreateInterpreterViewModel model)
        {
            ViewBag.Languages = new SelectList(new List<string> { "English", "Vietnamese", "Chinese", "Japanese", "Korean" });
            ViewBag.Types = new SelectList(new List<string> { "Simultaneous Interpreting", "Travel", "Relay Interpreting", "Whispering Interpreting" });
            ViewBag.Locations = new SelectList(new List<string> { "Da Nang  ", "Hoi An ", "Quang Nam ", "Hue ", "Quang Tri" });
            if (ModelState.IsValid)
            {
                var interpreter = new Interpreters
                {
                    FullName = model.FullName,
                    Rating = model.Rating,
                    Experience = model.Experience,
                    HourlyRate = model.HourlyRate,
                    Location = model.Location,
                    ContactInfo = model.ContactInfo,
                    Language = model.Language,
                    Type = model.Type,
                    Email = model.Email,
                    Password = model.Password,
                    Availability = model.Availability,
                };
                string uniqueFileName = null;

                // Xử lý tải ảnh
                if (model.Image != null)
                {
                    string uploadsFolder = Path.Combine(_webHostEnvironment.WebRootPath, "uploads");

                    // Tạo thư mục nếu chưa tồn tại
                    if (!Directory.Exists(uploadsFolder))
                    {
                        Directory.CreateDirectory(uploadsFolder);
                    }

                    // Đảm bảo tên file là duy nhất bằng cách thêm GUID
                    uniqueFileName = Guid.NewGuid().ToString() + "_" + model.Image.FileName;
                    string filePath = Path.Combine(uploadsFolder, uniqueFileName);

                    try
                    {
                        // Lưu file vào server
                        using (var fileStream = new FileStream(filePath, FileMode.Create))
                        {
                            await model.Image.CopyToAsync(fileStream);
                        }

                        // Lưu tên file ảnh vào đối tượng Interpreter
                        interpreter.Image = "/uploads/" + uniqueFileName;
                    }
                    catch (Exception ex)
                    {
                        // Ghi log lỗi hoặc hiển thị thông báo lỗi
                        ModelState.AddModelError("", "Unable to upload the image. " + ex.Message);



                        return View("~/Views/Admin/InterpreterManage/Create.cshtml");
                    }
                }


                _context.Add(interpreter);
                await _context.SaveChangesAsync();

                return RedirectToAction(nameof(Index));
            }
 
            return View("~/Views/Admin/InterpreterManage/Create.cshtml");
       }



        [Authentication]


        // GET: Interpreters/Edit/5
        public IActionResult Edit(int id)
        {
            var interpreter = _context.Interpreters.FirstOrDefault(i => i.InterpreterId == id);
            if (interpreter == null)
            {
                return NotFound();
            }
            ViewBag.Languages = new SelectList(new List<string> { "English", "Vietnamese", "Chinese", "Japanese", "Korean" });
            ViewBag.Types = new SelectList(new List<string> { "Simultaneous Interpreting", "Travel", "Relay Interpreting", "Whispering Interpreting" });
            ViewBag.Locations = new SelectList(new List<string> { "Da Nang  ", "Hoi An ", "Quang Nam ", "Hue ", "Quang Tri" });
            return View("~/Views/Admin/InterpreterManage/Edit.cshtml", interpreter);
        }

        // POST: Interpreters/Edit/5
        [Authentication]

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Edit(int id, Interpreters interpreter)
        {
            ViewBag.Languages = new SelectList(new List<string> { "English", "Vietnamese", "Chinese", "Japanese", "Korean" });
            ViewBag.Types = new SelectList(new List<string> { "Simultaneous Interpreting", "Travel", "Relay Interpreting", "Whispering Interpreting" });
            ViewBag.Locations = new SelectList(new List<string> { "Da Nang  ", "Hoi An ", "Quang Nam ", "Hue ", "Quang Tri" });
            if (id != interpreter.InterpreterId)
            {
                return NotFound();
            }

                try
                {
                    _context.Interpreters.Update(interpreter);
                    _context.SaveChanges();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!_context.Interpreters.Any(e => e.InterpreterId == id))
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

        // GET: Interpreters/Delete/5
        [Authentication]

        public IActionResult Delete(int id)
        {
            var interpreter = _context.Interpreters.FirstOrDefault(i => i.InterpreterId == id);
            if (interpreter == null)
            {
                return NotFound();
            }
            return View("~/Views/Admin/InterpreterManage/Delete.cshtml", interpreter);
        }
        [Authentication]

        // POST: Interpreters/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public IActionResult DeleteConfirmed(int id)
        {
            var interpreter = _context.Interpreters.FirstOrDefault(i => i.InterpreterId == id);
            if (interpreter != null)
            {
                _context.Interpreters.Remove(interpreter);
                _context.SaveChanges();
            }
            return RedirectToAction(nameof(Index));
        }
    
}
}
