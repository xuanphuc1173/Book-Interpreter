using EXE.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Linq;

namespace EXE.Controllers
{
    public class User : Controller
    {
        private readonly Exe101Context _context;
        public User(Exe101Context context)
        {
            _context = context;
        }
        public IActionResult Searchinterpreter()
        {
            return View();
        }
        public IActionResult Detailinterpreter()
        {
            return View();
        }
        public IActionResult Blog()

        {
            var blog = _context.Blogs.ToList();

            return View(blog);
        }
        public async Task<IActionResult> BlogDetail(int id)
        {
            var blog = _context.Blogs.FirstOrDefault(i => i.BlogId == id);
            if (blog == null)
            {
                return NotFound();
            }
            return View("~/Views/User/BlogDetail.cshtml", blog);
        }

        public IActionResult Appointment()
        {
            return View();
        }
        public IActionResult Pay()
        {
            return View();
        }
        public IActionResult Booking()
        {

            return View();
        }
        public IActionResult Premium()
        {
            return View();
        }


    }
}
