using EXE.Models;
using EXE.Models.Authentication;
using EXE.ViewModels;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Threading.Tasks;

namespace EXE.Controllers
{
    public class ChangePasswordController : Controller
    {
        private readonly Exe101Context _context;

		public ChangePasswordController(Exe101Context context)
		{
			_context = context;
		}
		[Authentication]
		[HttpGet]
        public IActionResult ChangePassword(int id)
        {
            var user = _context.Account.FirstOrDefault(u => u.AccountId == id);
            if (user == null)
            {
                return NotFound();
            }

            var model = new ChangePasswordViewModel
            {
                AccountId = user.AccountId
            };

            return View("~/Views/ChangePassword/ChangePassword.cshtml", model);
        }

        [Authentication]
		[HttpPost]
		public IActionResult ChangePassword(ChangePasswordViewModel model)
		{
            // Kiểm tra tính hợp lệ của model
            if (!ModelState.IsValid)
            {
                return View("ChangePassword", model); // Trả về view với model hiện tại nếu có lỗi xảy ra
            }
           
				var user = _context.Account.FirstOrDefault(u => u.AccountId == model.AccountId);
				if (user == null)
				{
					return NotFound();
				}
            if (user.Password == model.NewPassword)
            {
                ModelState.AddModelError("NewPassword", "New password cannot be the same as the current password.");
                return View("ChangePassword", model);
            }
            // Kiểm tra mật khẩu hiện tại
            if (user.Password != model.CurrentPassword)
				{
					ModelState.AddModelError("CurrentPassword", "Current password is incorrect.");
                return View("ChangePassword", model);
            }

				// Cập nhật mật khẩu mới
				user.Password = model.NewPassword;
				_context.Account.Update(user);
				_context.SaveChanges();

				TempData["SuccessMessage"] = "Your password has been successfully changed.";
				return RedirectToAction("ChangePassword", new { id = model.AccountId });
		
		}
	}
}
