
using EXE.Models;
using Microsoft.AspNetCore.Localization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Diagnostics;
using System.Globalization;

namespace EXE.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;
		private readonly Exe101Context _context;


		public HomeController(ILogger<HomeController> logger, Exe101Context context )
        {
            _logger = logger;
			_context = context;

		}

		public IActionResult Index()
        {
            return View();
        }
        public IActionResult Home()
        {
			var accountId = HttpContext.Session.GetInt32("AccountId");
			if (accountId != null)
			{
				var points = _context.Points.FirstOrDefault(p => p.AccountId == accountId);
				ViewBag.TotalPoints = points?.TotalPoints ?? 0; 
				ViewBag.InvitationCode = points?.InvitationCode;  
			}
			return View();
        }
        public IActionResult Lo()
        {
            return View();
        }
        public IActionResult Privacy()
        {
            return View();
        }
        public IActionResult SetLanguage(string lang ) 
        {
            if (!string.IsNullOrEmpty(lang))
            {
                Thread.CurrentThread.CurrentCulture = CultureInfo.CreateSpecificCulture(lang);
                Thread.CurrentThread.CurrentCulture = new CultureInfo(lang);
            }else
            {
                Thread.CurrentThread.CurrentCulture = CultureInfo.CreateSpecificCulture("en");
                Thread.CurrentThread.CurrentCulture = new CultureInfo("en");
                lang = "en";
            }
            Response.Cookies.Append("Language", lang);
            return Redirect(Request.GetTypedHeaders().Referer.ToString());
        }
      
    
    }
}
