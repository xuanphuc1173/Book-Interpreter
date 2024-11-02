using EXE.Models;
using EXE.ViewModels;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication.Google;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.VisualStudio.Web.CodeGenerators.Mvc.Templates.BlazorIdentity.Pages.Manage;
using System.ComponentModel.DataAnnotations;
using System.Security.Claims;
using Microsoft.AspNetCore.Authentication.Facebook;
using EXE.Models.Authentication;

namespace EXE.Controllers
{
    public class AccountController : Controller
    {
        private readonly Exe101Context _context;

        public AccountController(Exe101Context context)
        {
            _context = context;
        }

        public ActionResult Login()
        {
			return View("~/Views/Account/Login.cshtml");
		}

        // POST: Login
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Login(AccountViewModel userlogin)
        {
            if (ModelState.IsValid)
            {
                // Kiểm tra tài khoản trong bảng Account
                var userAccount = _context.Account.FirstOrDefault(u => u.Email == userlogin.Email && u.Password == userlogin.Password);

                if (userAccount != null)
				{
					TempData["SuccessMessage"] = "Login successful!"; // Thông báo chính
					TempData["UserName"] = "Welcome " + userAccount.FullName;
					// Thiết lập session cho tài khoản
					HttpContext.Session.SetString("Email", userAccount.Email);
                    HttpContext.Session.SetInt32("Role", userAccount.Role);
                    HttpContext.Session.SetString("UserName", userAccount.FullName);

                    if (userAccount.Role == 1) // Admin
                    {
                        TempData["SuccessMessage"] = "Login successful!"; // Thông báo chính
                        TempData["UserName"] = "Welcome " + userAccount.FullName;
                        return RedirectToAction("AdminHome", "Admin");
                    }
                    else if (userAccount.Role == 0) // User
                    {
                        TempData["SuccessMessage"] = "Login successful!"; // Thông báo chính
                        TempData["UserName"] = "Welcome " + userAccount.FullName; // Thông báo chào mừng
                        HttpContext.Session.SetInt32("AccountId", userAccount.AccountId);

                        return RedirectToAction("Home", "Home");
                    }
                }

                // Kiểm tra tài khoản trong bảng Interpreters
                var interpreter = _context.Interpreters.FirstOrDefault(i => i.Email == userlogin.Email && i.Password == userlogin.Password);

                if (interpreter != null)
                {
					TempData["SuccessMessage"] = "Login successful!"; // Thông báo chính
					TempData["UserName"] = "Welcome " + interpreter.FullName; 
                    // Thông báo chào mừng
																			  // Thiết lập session cho interpreter
					HttpContext.Session.SetString("Email", interpreter.Email);
                    HttpContext.Session.SetInt32("Role", 2); // Giả sử 2 là role cho Interpreter
                    HttpContext.Session.SetString("UserName", interpreter.FullName);
                    HttpContext.Session.SetInt32("InterpreterId", interpreter.InterpreterId); // Lưu InterpreterId
                    return RedirectToAction("Home", "Home"); // Trang chính cho Interpreter
                }

                // Nếu đăng nhập không thành công
                ModelState.AddModelError("", "Invalid email or password");
                return View(userlogin); // Giữ lại dữ liệu đã nhập
            }

            // Nếu dữ liệu không hợp lệ
            ModelState.AddModelError("", "Invalid email or password");
            return View(userlogin); // Giữ lại dữ liệu đã nhập
        }


        public ActionResult Register()
		{
			return View();
		}

		// POST: Register
		[HttpPost]
		[ValidateAntiForgeryToken]
		public ActionResult Register(RegisterViewModel model, string invitationCode = null)
		{
			if (!ModelState.IsValid)
			{
				return View(model);
			}

			try
			{
				var existingUser = _context.Account.FirstOrDefault(u => u.Email == model.Email);
				if (existingUser != null)
				{
					ModelState.AddModelError("Email", "Email is already in use.");
					ViewBag.ActiveTab = "register";
					return View(model);
				}

				if (model.Password != model.ConfirmPassword)
				{
					ModelState.AddModelError("ConfirmPassword", "Password and Confirmation Password must match.");
					ViewBag.ActiveTab = "register";
					return View(model);
				}

				var newUser = new Account
				{
					FullName = model.FullName,
					Email = model.Email,
					Phone = model.Phone,
					Password = model.Password,
					Role = 0 // Giả sử 0 là Role cho User
				};

				_context.Account.Add(newUser);
				_context.SaveChanges();

				// Tạo mã mời ngẫu nhiên
				var newInvitationCode = Guid.NewGuid().ToString();

				// Tạo bản ghi Points cho tài khoản mới
				var points = new Points
				{
					AccountId = newUser.AccountId,
					TotalPoints = 0, // Khởi tạo với 0 điểm
					InvitationCode = newInvitationCode // Lưu mã mời mới
				};

				// Kiểm tra mã mời hợp lệ
				if (!string.IsNullOrEmpty(invitationCode))
				{
					var existingPoints = _context.Points.FirstOrDefault(p => p.InvitationCode == invitationCode);
					if (existingPoints != null)
					{
						existingPoints.TotalPoints += 10; // Cộng thêm 10 điểm vào tài khoản tương ứng
						_context.SaveChanges(); // Lưu thay đổi
					}
					else
					{
						// Nếu mã mời không hợp lệ, thêm lỗi vào ModelState
						ModelState.AddModelError("", "Invalid invitation code.");
						ViewBag.ActiveTab = "register";
						return View(model);
					}
				}

				// Lưu bản ghi Points
				_context.Points.Add(points);
				_context.SaveChanges(); // Lưu thay đổi cho Points

				TempData["SuccessMessage"] = "Account created successfully. Please login.";
				return RedirectToAction("Login", "Account");
			}
			catch (Exception ex)
			{
				Console.WriteLine($"Error during registration: {ex.Message}");
				ModelState.AddModelError("", "An error occurred during registration. Please try again.");
				return View(model);
			}
		}


		[Authentication]

        [HttpGet]
        public async Task<IActionResult> Index()
        {
            var account = _context.Account.ToList();

            return View("~/Views/Admin/AccountManage/Index.cshtml", account);
        }
        public IActionResult Details(int id)
        {
            var account = _context.Account.FirstOrDefault(i => i.AccountId == id);
            if (account == null)
            {
                return NotFound();
            }
            return View("~/Views/Admin/AccountManage/Details.cshtml", account);
        }

        [HttpGet]
        public IActionResult Create()
        {
            return View("~/Views/Admin/AccountManage/Create.cshtml");
        }
        [Authentication]

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(CreateAccountViewModel model)
        {
            if (ModelState.IsValid)
            {
                var account = new Account
                {
                    FullName = model.FullName,
                    Email = model.Email,
                    Password = model.Password,
                    Phone = model.Phone
                };
                _context.Add(account);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            return View("~/Views/Admin/AccountManage/Create.cshtml", model);
        }
        [Authentication]

        public IActionResult Edit(int id)
        {
            var account = _context.Account.FirstOrDefault(i => i.AccountId == id);
            if (account == null)
            {
                return NotFound();
            }
            return View("~/Views/Admin/AccountManage/Edit.cshtml", account);
        }
        [Authentication]

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Edit(int id, Account account)
        {
            if (id != account.AccountId)
            {
                return NotFound();
            }

                try
                {
                    _context.Update(account);
                    _context.SaveChanges();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!_context.Account.Any(e => e.AccountId == id))
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

        [HttpGet]
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var account = await _context.Account
                .FirstOrDefaultAsync(m => m.AccountId == id);

            if (account == null)
            {
                return NotFound();
            }

            return View("~/Views/Admin/AccountManage/Delete.cshtml", account);
        }
        [Authentication]

        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var account = await _context.Account.FindAsync(id);
            if (account != null)
            {
                _context.Account.Remove(account);
                await _context.SaveChangesAsync();
            }

            return RedirectToAction(nameof(Index));
        }
		[HttpGet]
		public IActionResult GoogleLogin()
		{
			var properties = new AuthenticationProperties { RedirectUri = Url.Action("GoogleResponse") };
			return Challenge(properties, GoogleDefaults.AuthenticationScheme);
		}

		[HttpGet]
		public async Task<IActionResult> GoogleResponse()
		{
			var result = await HttpContext.AuthenticateAsync(CookieAuthenticationDefaults.AuthenticationScheme);

			if (result?.Principal != null)
			{
				var claims = result.Principal.Identities.FirstOrDefault()?.Claims;
				var email = claims?.FirstOrDefault(c => c.Type == ClaimTypes.Email)?.Value;
				var name = claims?.FirstOrDefault(c => c.Type == ClaimTypes.Name)?.Value;

				var user = _context.Account.FirstOrDefault(u => u.Email == email);
				if (user == null)
				{
					user = new Account
					{
						FullName = name,
						Email = email,
						Password = Guid.NewGuid().ToString(),
						Role = 0, 
					};

					_context.Account.Add(user);
					await _context.SaveChangesAsync();
				}

				HttpContext.Session.SetString("Email", user.Email);
				HttpContext.Session.SetInt32("Role", user.Role);
				HttpContext.Session.SetInt32("AccountId", user.AccountId);

				return RedirectToAction("Home", "Home");
			}

			return RedirectToAction("Login");
		}
		[HttpGet]
		public IActionResult FacebookLogin()
		{
			var properties = new AuthenticationProperties { RedirectUri = Url.Action("FacebookResponse") };
			return Challenge(properties, FacebookDefaults.AuthenticationScheme);
		}

		[HttpGet]
		public async Task<IActionResult> FacebookResponse()
		{
			var result = await HttpContext.AuthenticateAsync(CookieAuthenticationDefaults.AuthenticationScheme);

			if (result?.Principal != null)
			{
				var claims = result.Principal.Identities.FirstOrDefault()?.Claims;
				var email = claims?.FirstOrDefault(c => c.Type == ClaimTypes.Email)?.Value;
				var name = claims?.FirstOrDefault(c => c.Type == ClaimTypes.Name)?.Value;

				var user = _context.Account.FirstOrDefault(u => u.Email == email);
				if (user == null)
				{
					user = new Account
					{
						FullName = name,
						Email = email,
						Password = Guid.NewGuid().ToString(), 
						Role = 0 
					};

					_context.Account.Add(user);
					await _context.SaveChangesAsync();
				}

				HttpContext.Session.SetString("Email", user.Email);
				HttpContext.Session.SetInt32("Role", user.Role);
				HttpContext.Session.SetInt32("AccountId", user.AccountId);

				return RedirectToAction("Home", "Home");
			}

			return RedirectToAction("Login");
		}

		public ActionResult Logout()
        {
            HttpContext.Session.Clear();
            HttpContext.Session.Remove("Email");
            HttpContext.Session.Remove("Role");
            return RedirectToAction("Home", "Home");
        }
    }
}