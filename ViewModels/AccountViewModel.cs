using System.ComponentModel.DataAnnotations;

namespace EXE.ViewModels
{
    public class AccountViewModel
    {
		public int AccountId { get; set; }

		[Required(ErrorMessage = "Email is required.")]
		[EmailAddress(ErrorMessage = "Invalid email address.")]
		public string Email { get; set; } = null!;

		[Required(ErrorMessage = "Password is required.")]
		[DataType(DataType.Password)]
		public string Password { get; set; } = null!;
        public bool RememberMe { get; set; }
    }
}
