using System.ComponentModel.DataAnnotations;

namespace lern.Models;

public sealed class ChangePasswordViewModel
{
    [Required(ErrorMessage = "رمز عبور فعلی را وارد کنید.")]
    public string CurrentPassword { get; set; } = "";

    [Required(ErrorMessage = "رمز عبور جدید را وارد کنید.")]
    [MinLength(8, ErrorMessage = "رمز عبور جدید باید حداقل ۸ کاراکتر باشد.")]
    public string NewPassword { get; set; } = "";

    [Required(ErrorMessage = "تکرار رمز عبور جدید را وارد کنید.")]
    [Compare(nameof(NewPassword), ErrorMessage = "تکرار رمز عبور جدید مطابقت ندارد.")]
    public string ConfirmPassword { get; set; } = "";
}
