using System.ComponentModel.DataAnnotations;

namespace lern.Models;

public sealed class AdminUserEditViewModel
{
    public int Id { get; set; }

    [Required(ErrorMessage = "نام الزامی است.")]
    [StringLength(100)]
    public string FirstName { get; set; } = string.Empty;

    [Required(ErrorMessage = "نام خانوادگی الزامی است.")]
    [StringLength(100)]
    public string LastName { get; set; } = string.Empty;

    [Required(ErrorMessage = "شماره موبایل الزامی است.")]
    [RegularExpression(@"^09\d{9}$", ErrorMessage = "شماره موبایل باید با 09 شروع شود و 11 رقم باشد.")]
    public string PhoneNumber { get; set; } = string.Empty;

    [EmailAddress(ErrorMessage = "آدرس ایمیل معتبر نیست.")]
    [StringLength(256)]
    public string? Email { get; set; }

    [MinLength(8, ErrorMessage = "رمز عبور جدید باید حداقل ۸ کاراکتر باشد.")]
    [DataType(DataType.Password)]
    public string? Password { get; set; }

    [Compare(nameof(Password), ErrorMessage = "رمز عبور و تکرار آن یکسان نیستند.")]
    [DataType(DataType.Password)]
    public string? ConfirmPassword { get; set; }

    [Required]
    public string Role { get; set; } = "Customer";
    public bool IsActive { get; set; }
    public string? ExistingAvatarUrl { get; set; }
    public IFormFile? Avatar { get; set; }
}
