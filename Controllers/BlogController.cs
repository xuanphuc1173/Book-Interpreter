using EXE.Models;
using EXE.Models.Authentication;
using EXE.ViewModels;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Net.Http;
using System.Reflection.Metadata;

namespace EXE.Controllers
{
    public class BlogController : Controller
    {
        private readonly Exe101Context _context;
        private readonly IWebHostEnvironment _webHostEnvironment;
        public BlogController(Exe101Context context, IWebHostEnvironment webHostEnvironment)
        {
            _context = context;
            _webHostEnvironment = webHostEnvironment;
        }
        [Authentication]
        public async Task<IActionResult> Index()
        {
            var blog = _context.Blogs.ToList();
            return View("~/Views/Admin/BlogManage/Index.cshtml", blog);
        }
        // GET: Category/Details/5
        public async Task<IActionResult> Details(int id)
        {
            var blog = _context.Blogs.FirstOrDefault(i => i.BlogId == id);
            if (blog == null)
            {
                return NotFound();
            }
            return View("~/Views/Admin/BlogManage/Details.cshtml", blog);
        }
        // GET: Category/Create
        public IActionResult Create()
        {
            return View("~/Views/Admin/BlogManage/Create.cshtml");
        }

        // POST: Category/Create
        [Authentication]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(CreateBlogViewModel model)
        {
            if (ModelState.IsValid)
            {
                var blog = new Blog
                {
                    Title = model.Title,
                    Contents = model.Contents,
                    Contents2 = model.Contents2,
                    Contents3 = model.Contents3,
                    PublishedDate = DateTime.Now              
                };

                string uniqueFileName = null;

                // Xử lý tải ảnh đầu tiên
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

                        blog.Image = "/uploads/" + uniqueFileName;
                    }
                    catch (Exception ex)
                    {
                        // Ghi log lỗi hoặc hiển thị thông báo lỗi
                        ModelState.AddModelError("", "Unable to upload the image. " + ex.Message);
                        return View("~/Views/Admin/BlogManage/Create.cshtml");
                    }
                }

                // Xử lý tải ảnh thứ hai
                if (model.Image2 != null)
                {
                    string uploadsFolder = Path.Combine(_webHostEnvironment.WebRootPath, "uploads");

                    // Đảm bảo tên file là duy nhất bằng cách thêm GUID
                    uniqueFileName = Guid.NewGuid().ToString() + "_" + model.Image2.FileName;
                    string filePath = Path.Combine(uploadsFolder, uniqueFileName);

                    try
                    {
                        // Lưu file vào server
                        using (var fileStream = new FileStream(filePath, FileMode.Create))
                        {
                            await model.Image2.CopyToAsync(fileStream);
                        }

                        blog.Image2 = "/uploads/" + uniqueFileName;
                    }
                    catch (Exception ex)
                    {
                        // Ghi log lỗi hoặc hiển thị thông báo lỗi
                        ModelState.AddModelError("", "Unable to upload the image. " + ex.Message);
                        return View("~/Views/Admin/BlogManage/Create.cshtml");
                    }
                }

                // Xử lý tải ảnh thứ ba
                if (model.Image3 != null)
                {
                    string uploadsFolder = Path.Combine(_webHostEnvironment.WebRootPath, "uploads");

                    // Đảm bảo tên file là duy nhất bằng cách thêm GUID
                    uniqueFileName = Guid.NewGuid().ToString() + "_" + model.Image3.FileName;
                    string filePath = Path.Combine(uploadsFolder, uniqueFileName);

                    try
                    {
                        // Lưu file vào server
                        using (var fileStream = new FileStream(filePath, FileMode.Create))
                        {
                            await model.Image3.CopyToAsync(fileStream);
                        }

                        blog.Image3 = "/uploads/" + uniqueFileName;
                    }
                    catch (Exception ex)
                    {
                        // Ghi log lỗi hoặc hiển thị thông báo lỗi
                        ModelState.AddModelError("", "Unable to upload the image. " + ex.Message);
                        return View("~/Views/Admin/BlogManage/Create.cshtml");
                    }
                }

                _context.Add(blog);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            return View("~/Views/Admin/BlogManage/Create.cshtml", model);
        }

        [Authentication]

        // GET: Category/Edit/5
        public async Task<IActionResult> Edit(int id)
        {
            var blog = _context.Blogs.FirstOrDefault(i => i.BlogId == id);
            if (blog == null)
            {
                return NotFound();
            }
            return View("~/Views/Admin/BlogManage/Edit.cshtml", blog);
        }

        // POST: Category/Edit/5
        [Authentication]

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, Blog blog)
        {
            if (id != blog.BlogId)
            {
                return NotFound();
            }

            try
            {
                _context.Blogs.Update(blog);
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




        // GET: Category/Delete/5
        [Authentication]

        public async Task<IActionResult> Delete(int id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var blog = await _context.Blogs
                .FirstOrDefaultAsync(m => m.BlogId == id);

            if (blog == null)
            {
                return NotFound();
            }

            return View("~/Views/Admin/BlogManage/Delete.cshtml", blog);
        }

        // POST: Category/Delete/5
        [Authentication]

        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var blog = await _context.Blogs.FindAsync(id);
            if (blog != null)
            {
                _context.Blogs.Remove(blog);
                await _context.SaveChangesAsync();
            }

            return RedirectToAction(nameof(Index));
        }
    }
}
