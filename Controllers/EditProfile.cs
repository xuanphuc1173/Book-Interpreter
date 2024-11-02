using EXE.Models;
using EXE.Models.Authentication;
using Microsoft.AspNetCore.Mvc;
using System.Text.RegularExpressions;

namespace EXE.Controllers
{
    public class EditProfile : Controller
    {
        private readonly Exe101Context _context;

        public EditProfile(Exe101Context context)
        {
            _context = context;
        }

        [Authentication]
        public IActionResult Index(int? id)
        {
            if (TempData["SuccessMessage"] != null)
            {
                ViewBag.SuccessMessage = TempData["SuccessMessage"].ToString();
            }
            var user = _context.Account.FirstOrDefault(t => t.AccountId == id);
            if (user != null)
            {
                var result = new Account()
                {
                    AccountId = user.AccountId,
                    FullName = user.FullName,
                    Phone = user.Phone,
                    Email = user.Email,
                };


                return View(result);
            }

            // Trả về một danh sách trống nếu không tìm thấy user
            return View();
        }
		[Authentication]
		[HttpPost]
		public IActionResult SaveChanges(Account model)
		{
			// Kiểm tra tính hợp lệ của ModelState
			if (!ModelState.IsValid)
			{
				// Nếu không hợp lệ, trả về view với model hiện tại
				return View("Index", model);
			}

			// Kiểm tra các điều kiện lỗi tùy chỉnh
			if (!string.IsNullOrEmpty(model.Phone) && !Regex.IsMatch(model.Phone, @"^0\d{9}$"))
			{
				ModelState.AddModelError("Phone", "Phone number must contain exactly 10 digits and start with 0.");
				return View("Index", model); // Trả về view với model hiện tại nếu có lỗi
			}
			if (!string.IsNullOrEmpty(model.Email) && !Regex.IsMatch(model.Email, @"^[^@\s]+@[^@\s]+\.(com)$"))
			{
				ModelState.AddModelError("Email", "Email must contain @ and end with .com.");
				return View("Index", model); // Trả về view với model hiện tại nếu có lỗi
			}
			if (!string.IsNullOrEmpty(model.FullName) && !Regex.IsMatch(model.FullName, @"^[a-zA-ZÀ-ỹ\s]*$"))
			{
				ModelState.AddModelError("FullName", "Full name can only be entered in letters and spaces.");
				return View("Index", model); // Trả về view với model hiện tại nếu có lỗi
			}

			// Lấy người dùng từ cơ sở dữ liệu
			var user = _context.Account.FirstOrDefault(u => u.AccountId == model.AccountId);
			if (user == null)
			{
				ModelState.AddModelError("", "User not found.");
				return View("Index", model);
			}

			// Cập nhật thông tin người dùng
			user.FullName = model.FullName;
			user.Phone = model.Phone;
			user.Email = model.Email;

			try
			{
				_context.Account.Update(user); // Explicitly mark as updated
				_context.SaveChanges(); // Lưu thay đổi
			}
			catch (Exception ex)
			{
				// Ghi log lỗi và thông báo cho người dùng
				ModelState.AddModelError("", "An error occurred while saving changes: " + ex.Message);
				return View("Index", model);
			}

			TempData["SuccessMessage"] = "Your information has been updated successfully!";
			return RedirectToAction("Index", new { id = model.AccountId });
		}

	}
}
