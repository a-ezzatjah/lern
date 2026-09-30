using Entities;
using lern.Infrastructure;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Data.SqlClient;
using System.Security.Claims;

namespace lern.Controller;

[ApiController, Route("api/orders")]
public class OrderController : ControllerBase
{
    private readonly ShopDbContext _db;
    public OrderController(ShopDbContext db) => _db = db;
    private string Key => Request.Cookies["customer-key"] ?? "guest";

    [HttpGet]
    public async Task<IActionResult> Mine() => Ok(await _db.Orders.AsNoTracking().Include(x => x.Items)
        .Where(x => x.CustomerKey == Key).OrderByDescending(x => x.CreatedAt)
        .Select(x => new { x.Id, x.Status, x.Total, x.CreatedAt, Items = x.Items.Select(i => new { i.ProductName, i.Quantity, i.UnitPrice }) }).ToListAsync());

    [Authorize]
    [HttpPost("coupon/preview")]
    public async Task<IActionResult> PreviewCoupon(CouponPreviewRequest request)
    {
        if (!int.TryParse(User.FindFirstValue(ClaimTypes.NameIdentifier), out var userId) || Key != $"user-{userId}"
            || !await _db.Users.AnyAsync(x => x.Id == userId && x.IsActive))
            return BadRequest("برای استفاده از کوپن، دوباره وارد حساب خود شوید.");
        var cart = await _db.CartItems.AsNoTracking().Include(x => x.Product).Include(x => x.ProductVariant)
            .Where(x => x.CustomerKey == Key).ToListAsync();
        if (cart.Count == 0) return BadRequest("سبد خرید خالی است.");
        var now = DateTime.UtcNow;
        var subtotal = cart.Sum(x => CartPricing.GetFinalPrice(x.Product, x.ProductVariant, now) * x.Quantity);
        var (coupon, error, discount) = await ValidateCoupon(request.Code, userId, subtotal, now);
        if (error is not null) return BadRequest(error);
        var shipping = coupon!.Kind == CouponKind.FreeShipping ? 0m : 300000m;
        return Ok(new { coupon.Code, Discount = discount, Shipping = shipping, Total = subtotal + shipping - discount });
    }

    [HttpPost("checkout")]
    public async Task<IActionResult> Checkout(CheckoutRequest request)
    {
        if (Request.Cookies["customer-key"] is null) return BadRequest("شناسه مشتری وجود ندارد؛ ابتدا سبد خرید را دریافت کنید.");
        if (request.ShippingCost != 300000)
            return BadRequest("هزینه ارسال انتخاب‌شده معتبر نیست.");
        if (string.IsNullOrWhiteSpace(request.FirstName) || string.IsNullOrWhiteSpace(request.LastName) || string.IsNullOrWhiteSpace(request.Phone))
            return BadRequest("لطفاً نام، نام خانوادگی و شماره تماس را وارد کنید.");
        var hasCoupon = !string.IsNullOrWhiteSpace(request.CouponCode);
        var userId = 0;
        if (hasCoupon && (!int.TryParse(User.FindFirstValue(ClaimTypes.NameIdentifier), out userId)
            || Key != $"user-{userId}" || !await _db.Users.AnyAsync(x => x.Id == userId && x.IsActive)))
            return BadRequest("برای استفاده از کوپن باید با حساب فعال خود وارد شوید.");
        await using var tx = await _db.Database.BeginTransactionAsync();
        var cart = await _db.CartItems.Include(x => x.Product).Include(x => x.ProductVariant)
            .Where(x => x.CustomerKey == Key).ToListAsync();
        if (cart.Count == 0) return BadRequest("سبد خرید خالی است.");
        var order = new Order { CustomerKey = Key, Status = OrderStatus.Pending, CustomerFirstName = request.FirstName.Trim(), CustomerLastName = request.LastName.Trim(), CustomerPhone = request.Phone.Trim() };
        foreach (var c in cart)
        {
            var available = c.ProductVariant.StockQuantity - c.ProductVariant.ReservedQuantity;
            if (available < c.Quantity) return BadRequest($"موجودی «{c.Product.Name}» کافی نیست.");
            var unitPrice = CartPricing.GetFinalPrice(c.Product, c.ProductVariant, DateTime.UtcNow);
            var total = unitPrice * c.Quantity;
            order.Items.Add(new OrderItem { ProductId = c.ProductId, ProductVariantId = c.ProductVariantId, ProductName = c.Product.Name, UnitPrice = unitPrice, Quantity = c.Quantity, LineTotal = total });
            order.Subtotal += total;
            c.ProductVariant.ReservedQuantity += c.Quantity;
        }
        DiscountCoupon? coupon = null;
        var discount = 0m;
        if (hasCoupon)
        {
            var result = await ValidateCoupon(request.CouponCode!, userId, order.Subtotal, DateTime.UtcNow);
            if (result.Error is not null) return BadRequest(result.Error);
            coupon = result.Coupon;
            discount = result.Discount;
        }
        var shipping = coupon?.Kind == CouponKind.FreeShipping ? 0m : request.ShippingCost;
        order.CouponDiscount = discount;
        order.Total = order.Subtotal + shipping - discount;
        _db.Orders.Add(order); _db.CartItems.RemoveRange(cart);
        if (coupon is not null)
            _db.CouponRedemptions.Add(new CouponRedemption { CouponId = coupon.Id, UserId = userId,
                Order = order, DiscountAmount = coupon.Kind == CouponKind.FreeShipping ? request.ShippingCost : discount });
        try { await _db.SaveChangesAsync(); await tx.CommitAsync(); }
        catch (DbUpdateException ex) when (ex.InnerException is SqlException { Number: 2601 or 2627 } && coupon is not null)
        {
            return Conflict("این کد قبلاً برای حساب شما استفاده شده است.");
        }
        return Ok(new { order.Id, order.Total, order.Status, order.CouponDiscount, Shipping = shipping });
    }

    [HttpPost("{id}/pay")]
    public async Task<IActionResult> Pay(int id)
    {
        var order = await _db.Orders.Include(x => x.Items).ThenInclude(x => x.ProductVariant)
            .SingleOrDefaultAsync(x => x.Id == id && x.CustomerKey == Key);
        if (order is null) return NotFound();
        if (order.Status == OrderStatus.Paid) return Ok(new { order.Id, order.Status });
        foreach (var item in order.Items)
        {
            item.ProductVariant.ReservedQuantity = Math.Max(0, item.ProductVariant.ReservedQuantity - item.Quantity);
            item.ProductVariant.StockQuantity = Math.Max(0, item.ProductVariant.StockQuantity - item.Quantity);
        }
        order.Status = OrderStatus.Paid;
        _db.PaymentTransactions.Add(new PaymentTransaction { OrderId = id, Amount = order.Total, Status = PaymentStatus.Successful, Gateway = "manual", Reference = Guid.NewGuid().ToString("N") });
        await _db.SaveChangesAsync(); return Ok(new { order.Id, order.Status });
    }

    private async Task<(DiscountCoupon? Coupon, string? Error, decimal Discount)> ValidateCoupon(
        string? rawCode, int userId, decimal subtotal, DateTime now)
    {
        var code = rawCode?.Trim().ToUpperInvariant();
        if (string.IsNullOrWhiteSpace(code)) return (null, "کد تخفیف را وارد کنید.", 0);
        var coupon = await _db.DiscountCoupons.AsNoTracking().SingleOrDefaultAsync(x => x.Code == code);
        if (coupon is null || !coupon.IsActive || coupon.ExpiresAt <= now)
            return (null, "کد تخفیف معتبر یا فعال نیست.", 0);
        if (coupon.MinimumOrderAmount < 0 || coupon.Kind is CouponKind.Percentage && (coupon.Value <= 0 || coupon.Value > 100)
            || coupon.Kind is CouponKind.FixedAmount && coupon.Value <= 0)
            return (null, "تنظیمات این کد تخفیف معتبر نیست.", 0);
        if (coupon.RecipientUserId.HasValue && coupon.RecipientUserId != userId)
            return (null, "این کد برای حساب شما صادر نشده است.", 0);
        if (await _db.CouponRedemptions.AnyAsync(x => x.CouponId == coupon.Id && x.UserId == userId))
            return (null, "این کد قبلاً برای حساب شما استفاده شده است.", 0);
        if (subtotal < coupon.MinimumOrderAmount)
            return (null, "مبلغ سبد خرید به حداقل لازم برای این کد نرسیده است.", 0);
        var discount = coupon.Kind switch
        {
            CouponKind.Percentage => decimal.Round(subtotal * coupon.Value / 100m, 0, MidpointRounding.AwayFromZero),
            CouponKind.FixedAmount => Math.Min(subtotal, coupon.Value),
            CouponKind.FreeShipping => 0m,
            _ => -1m
        };
        if (discount < 0) return (null, "نوع کد تخفیف معتبر نیست.", 0);
        return (coupon, null, discount);
    }
}

public record CheckoutRequest(decimal ShippingCost, string FirstName = "", string LastName = "", string Phone = "", string? CouponCode = null);
public record CouponPreviewRequest(string Code);
