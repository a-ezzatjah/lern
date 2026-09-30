namespace Entities;

public class CouponRedemption
{
    public int Id { get; set; }
    public int CouponId { get; set; }
    public DiscountCoupon Coupon { get; set; } = null!;
    public int UserId { get; set; }
    public CustomerUser User { get; set; } = null!;
    public int OrderId { get; set; }
    public Order Order { get; set; } = null!;
    public decimal DiscountAmount { get; set; }
    public DateTime UsedAt { get; set; } = DateTime.UtcNow;
}
