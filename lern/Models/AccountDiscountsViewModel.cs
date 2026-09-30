using Entities;

namespace lern.Models;

public sealed class AccountDiscountsViewModel
{
    public List<DiscountCoupon> Coupons { get; init; } = new();
    public HashSet<int> UsedCouponIds { get; init; } = new();
}

public sealed class AccountDiscountCardViewModel
{
    public DiscountCoupon Coupon { get; init; } = null!;
    public bool IsUsed { get; init; }
}
