using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace EXE.Models;

public partial class Account
{
    public int AccountId { get; set; }

    public string FullName { get; set; } = null!;
    [RegularExpression(@"^0\d{9}$", ErrorMessage = "Phone number must contain exactly 10 digits and start with 0.")]
    public string? Phone { get; set; } = null!;
    [RegularExpression(@"^[^@\s]+@[^@\s]+\.(com)$", ErrorMessage = "Email must contain @ and end with .com.")]
    public string Email { get; set; } = null!;

    public string Password { get; set; } = null!;

    public int Role { get; set; }
}
