using System.ComponentModel.DataAnnotations;
using Entities;

namespace lern.Models;

public sealed class ComplaintViewModel
{
    private string _phone = "";
    private string? _orderNumber;
    private string _fullName = "";
    private string _message = "";

    [Required(ErrorMessage = "نام و نام خانوادگی را وارد کنید.")]
    [StringLength(100, MinimumLength = 3, ErrorMessage = "نام باید بین ۳ تا ۱۰۰ نویسه باشد.")]
    public string FullName { get => _fullName; set => _fullName = (value ?? "").Trim(); }

    [Required(ErrorMessage = "شماره موبایل را وارد کنید.")]
    [RegularExpression(@"09[0-9]{9}", ErrorMessage = "شماره موبایل باید ۱۱ رقم باشد و با ۰۹ شروع شود.")]
    public string Phone { get => _phone; set => _phone = NormalizeDigits(value); }

    [RegularExpression(@"[0-9]{1,12}", ErrorMessage = "شماره سفارش را فقط با عدد، حداکثر ۱۲ رقم وارد کنید.")]
    public string? OrderNumber { get => _orderNumber; set => _orderNumber = string.IsNullOrWhiteSpace(value) ? null : NormalizeDigits(value); }

    [Required(ErrorMessage = "موضوع درخواست را انتخاب کنید.")]
    [EnumDataType(typeof(ComplaintSubject), ErrorMessage = "موضوع درخواست معتبر نیست.")]
    public ComplaintSubject? Subject { get; set; }

    [Required(ErrorMessage = "شرح درخواست را وارد کنید.")]
    [StringLength(3000, MinimumLength = 15, ErrorMessage = "شرح درخواست باید بین ۱۵ تا ۳۰۰۰ نویسه باشد.")]
    public string Message { get => _message; set => _message = (value ?? "").Trim(); }

    // Empty for people; automated form submissions often fill this field.
    public string? Website { get; set; }

    private static string NormalizeDigits(string? value) => string.Concat((value ?? "").Trim().Select(c =>
        c is >= '۰' and <= '۹' ? (char)('0' + c - '۰') : c is >= '٠' and <= '٩' ? (char)('0' + c - '٠') : c));
}
