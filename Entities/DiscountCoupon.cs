namespace Entities;

public enum CouponKind { Percentage = 1, FixedAmount = 2, FreeShipping = 3 }

public class DiscountCoupon
{
    public int Id { get; set; }
    public string Code { get; set; } = null!;
    public string Title { get; set; } = null!;
    public CouponKind Kind { get; set; }
    public decimal Value { get; set; }
    public decimal MinimumOrderAmount { get; set; }
    public DateTime ExpiresAt { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public bool IsActive { get; set; } = true;
    // A null recipient means the coupon is available to every customer account.
    public int? RecipientUserId { get; set; }
    public CustomerUser? RecipientUser { get; set; }
}
