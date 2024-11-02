using EXE.Models;
using EXE.Models.Authentication;
using EXE.ViewModels;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Microsoft.Identity.Client;

namespace EXE.Controllers
{
    public class ReviewController : Controller
    {
        private readonly Exe101Context _context;
        private readonly IWebHostEnvironment _webHostEnvironment;
        public ReviewController(Exe101Context context, IWebHostEnvironment webHostEnvironment)
        {
            _context = context;
            _webHostEnvironment = webHostEnvironment;
        }
        [Authentication]

        public async Task<IActionResult> Index()
        {
            var reviews = await _context.Reviews
                .Include(b => b.Account)
                .Include(b => b.Interpreter)
                .ToListAsync();
            return View("~/Views/Admin/ReviewManage/Index.cshtml", reviews);
        }
        [HttpGet]
        public IActionResult CreateReview(int interpreterId)
        {
            // Lấy AccountId từ session
            var accountId = HttpContext.Session.GetInt32("AccountId");

            if (accountId.HasValue)
            {
                // Chuyển đổi thành int
                int actualAccountId = accountId.Value;

                // Sử dụng actualAccountId (kiểu int) trong truy vấn
                var booking = _context.Bookings
                    .FirstOrDefault(b => b.InterpreterId == interpreterId && b.AccountId == actualAccountId && DateTime.Now >= b.EndTime);

                // Kiểm tra xem đã có review cho thông dịch viên này chưa và booking đã hoàn thành chưa
                if (booking == null)
                {
                    return BadRequest("Bạn chỉ có thể đánh giá sau khi booking kết thúc.");
                }

                // Tạo model review để hiển thị form
                var reviewModel = new Reviews
                {
                    InterpreterId = interpreterId,
                    AccountId = actualAccountId  // Sử dụng actualAccountId thay vì accountId
                };

                return View("~/Views/User/CreateReview.cshtml", reviewModel); // Hiển thị form đánh giá
            }
            else
            {
                // Xử lý khi không có AccountId trong session
                return BadRequest("AccountId is not set in session.");
            }
        }


        [HttpPost]
        public IActionResult CreateReview(Reviews review)
        {
            if (ModelState.IsValid)
            {
                review.ReviewDate = DateTime.Now;

                _context.Reviews.Add(review);
                _context.SaveChanges();

                var points = _context.Points.FirstOrDefault(p => p.AccountId == review.AccountId);
                if (points == null)
                {

                    points = new Points
                    {
                        AccountId = review.AccountId,
                        TotalPoints = 10 
                    };
                    _context.Points.Add(points);
                }
                else
                {
                    points.TotalPoints += 10;
                }

                _context.SaveChanges(); 

                return View("~/Views/Home/Home.cshtml", review);
            }

            return View(review);
        }



        [Authentication]

        public async Task<IActionResult> Details(int id)
        {
            var blog = await _context.Reviews                
                .Include(b => b.Account)
                .Include(b => b.Interpreter)
                .FirstOrDefaultAsync(m => m.ReviewId == id);
            if (blog == null)
            {
                return NotFound();
            }
            return View("~/Views/Admin/ReviewManage/Details.cshtml", blog);
        }
        // GET: Category/Create
        [Authentication]

        public async Task<IActionResult> Create()
        {
            ViewBag.Accounts = await _context.Account.ToListAsync();
            ViewBag.Ratings = new SelectList(new List<string> { "0.5","1", "1.5", "2", "2.5", "3","3.5","4.5","5" });

            // Lấy danh sách tất cả Interpreter để hiển thị
            ViewBag.Interpreters = await _context.Interpreters.ToListAsync();
            return View("~/Views/Admin/ReviewManage/Create.cshtml");
        }

        // POST: Category/Create
        [Authentication]

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(CreateReviewModel model)
        {

            if (ModelState.IsValid)
            {
                var review = new Reviews
                {
                    AccountId = model.AccountId,
                    InterpreterId = model.InterpreterId,
                    Rating = model.Rating,
                    Comment = model.Comment,
                    ReviewDate = DateTime.Now
                };

                _context.Add(review);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            return View("~/Views/Admin/ReviewManage/Create.cshtml", model);
        }


        // GET: Category/Edit/5
        [Authentication]

        public async Task<IActionResult> Edit(int id)
        {
            var review = await _context.Reviews.FirstOrDefaultAsync(i => i.ReviewId == id);
            if (review == null)
            {
                return NotFound();
            }

            // Lấy danh sách tất cả Account để hiển thị
            ViewBag.Accounts = await _context.Account.ToListAsync();
            ViewBag.Ratings = new SelectList(new List<string> { "0.5", "1", "1.5", "2", "2.5", "3", "3.5", "4.5", "5" });
            // Lấy danh sách tất cả Interpreter để hiển thị
            ViewBag.Interpreters = await _context.Interpreters.ToListAsync();

            return View("~/Views/Admin/ReviewManage/Edit.cshtml", review);
        }

        // POST: Category/Edit/5
        [Authentication]

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, Reviews reviews)
        {
            if (id != reviews.ReviewId)
            {
                return NotFound();
            }

            try
            {
                _context.Reviews.Update(reviews);
                _context.SaveChanges();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!_context.Blogs.Any(e => e.BlogId == id))
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


        [Authentication]


        // GET: Category/Delete/5
        public async Task<IActionResult> Delete(int id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var reviews = await _context.Reviews
                .FirstOrDefaultAsync(m => m.ReviewId == id);

            if (reviews == null)
            {
                return NotFound();
            }
            ViewBag.Accounts = await _context.Account.ToListAsync();
            ViewBag.Interpreters = await _context.Interpreters.ToListAsync();

            return View("~/Views/Admin/ReviewManage/Delete.cshtml", reviews);
        }
        [Authentication]

        // POST: Category/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var reviews = await _context.Reviews.FindAsync(id);
            if (reviews != null)
            {
                _context.Reviews.Remove(reviews);
                await _context.SaveChangesAsync();
            }

            return RedirectToAction(nameof(Index));
        }
    }
}
