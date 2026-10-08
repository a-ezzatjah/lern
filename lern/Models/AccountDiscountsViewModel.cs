using Entities;

namespace lern.Models;

public sealed class AccountDiscountsViewModel
{
    public List<DiscountCoupon> Coupons { get; init; } = new();
    public string Tab { get; init; } = "active";
    public int Page { get; init; } = 1;
    public int TotalCount { get; init; }
    public int PageSize => 10;
    public int TotalPages => (TotalCount + PageSize - 1) / PageSize;
}

public sealed class AccountDiscountCardViewModel
{
    public DiscountCoupon Coupon { get; init; } = null!;
    public bool IsUsed { get; init; }
}
