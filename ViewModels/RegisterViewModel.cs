using AppleStore.Pages.Client;
using System.ComponentModel.DataAnnotations;

namespace EXE.ViewModels
{
	public class RegisterViewModel
	{
		[Required]
		[RegularExpression(@"^[a-zA-Z\s]+$", ErrorMessage = "Full name can only contain letters and spaces.")]
		public string FullName { get; set; } = null!;

		[Required]
		[Phone]
		[StringLength(15, MinimumLength = 10, ErrorMessage = "Phone number must be at least 10 digits long.")]
        [RegularExpression(@"^0\d{9,14}$", ErrorMessage = "Phone number must start with 0 and contain only digits.")]
        public string Phone { get; set; } = null!;
		

		[Required]
		[GmailDomain(ErrorMessage = "User name must end with @gmail.com.")]

		public string Email { get; set; } 

		[Required]
		[DataType(DataType.Password)]
		[StringLength(100, MinimumLength = 8, ErrorMessage = "Password must be at least 8 characters long.")]
        [RegularExpression(@"^(?=.*[a-z])(?=.*[A-Z])(?=.*\d)(?=.*[@$!%*?&.])[A-Za-z\d@$!%*?&.]{8,}$",
			ErrorMessage = "Password must contain at least one uppercase letter, one lowercase letter, one digit, and one special character.")]
		public string Password { get; set; } = null!;
	
		[Required]
		[Compare("Password", ErrorMessage = "Password and Confirmation Password must match.")]
		[DataType(DataType.Password)]
		public string ConfirmPassword { get; set; } = null!;
		public int Role { get; set; }
        public string? InvitationCode { get; set; }
    }

}
