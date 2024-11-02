using System.ComponentModel.DataAnnotations;

namespace EXE.ViewModels
{
    public class CreateAccountViewModel
    {
        public int AccountId { get; set; }

        public string FullName { get; set; } = null!;
        
        public string Phone { get; set; } = null!;
        [RegularExpression(@"^[^@\s]+@[^@\s]+\.(com)$", ErrorMessage = "Email must contain @ and end with .com.")]
        public string Email { get; set; } = null!;

        public string Password { get; set; } = null!;
    }
}
