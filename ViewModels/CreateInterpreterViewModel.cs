using Microsoft.AspNetCore.Mvc.Rendering;
using System.ComponentModel.DataAnnotations;

namespace EXE.ViewModels
{
    public class CreateInterpreterViewModel
    {
        public string FullName { get; set; }
        public decimal Rating { get; set; }
        public int Experience { get; set; }
        public decimal HourlyRate { get; set; }
        public string Location { get; set; }
        [RegularExpression(@"^[^@\s]+@gmail\.com$", ErrorMessage = "Email must be in the format: example@gmail.com.")]

        public string Email { get; set; }
        public string Password { get; set; }
        public string ContactInfo { get; set; }
        public string Language { get; set; }
        public string Type  { get; set; }
        public bool Availability { get; set; }
        public IFormFile Image { get; set; }

    }
}
