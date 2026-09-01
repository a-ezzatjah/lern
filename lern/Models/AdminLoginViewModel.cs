using System.ComponentModel.DataAnnotations;

namespace lern.Models;

public class AdminLoginViewModel
{
    [Required(ErrorMessage = "نام کاربری را وارد کنید.")]
    public string Username { get; set; } = string.Empty;

    [Required(ErrorMessage = "رمز عبور را وارد کنید.")]
    [DataType(DataType.Password)]
    public string Password { get; set; } = string.Empty;

    public string? ReturnUrl { get; set; }
}
