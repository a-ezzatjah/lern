using System.ComponentModel.DataAnnotations;
using Entities;

namespace lern.Models;

public class AdminCouponCreateModel
{
    [Required(ErrorMessage = "عنوان کارت را وارد کنید.")]
    [StringLength(150)]
    public string Title { get; set; } = "";
    [Required(ErrorMessage = "کد کوپن را وارد کنید.")]
    [RegularExpression("^[A-Za-z0-9_-]{3,40}$", ErrorMessage = "کد باید ۳ تا ۴۰ نویسه انگلیسی، عدد، خط تیره یا زیرخط باشد.")]
    public string Code { get; set; } = "";
    [Required(ErrorMessage = "نوع تخفیف را انتخاب کنید.")]
    public CouponKind Kind { get; set; } = CouponKind.Percentage;
    [Range(0, 999999999, ErrorMessage = "مقدار تخفیف معتبر نیست.")]
    public decimal Value { get; set; }
    [Range(0, 999999999, ErrorMessage = "حداقل مبلغ سفارش معتبر نیست.")]
    public decimal MinimumOrderAmount { get; set; }
    [Required(ErrorMessage = "تاریخ انقضا را وارد کنید.")]
    public string ExpiresOnJalali { get; set; } = "";
    public int? RecipientUserId { get; set; }
    public bool SendToAll { get; set; }
}

public class AdminCouponPageModel
{
    public AdminCouponCreateModel Form { get; set; } = new();
    public List<DiscountCoupon> Coupons { get; set; } = new();
    public List<CustomerUser> Customers { get; set; } = new();
    public int Page { get; set; } = 1;
    public int PageSize { get; set; } = 10;
    public int TotalCount { get; set; }
    public int ActiveCount { get; set; }
    public int BroadcastCount { get; set; }
    public int TotalPages => Math.Max(1, (int)Math.Ceiling(TotalCount / (double)PageSize));
}
