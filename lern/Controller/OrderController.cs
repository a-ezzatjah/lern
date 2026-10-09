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
    private readonly CustomerSession _session;
    private readonly OrderReservations _reservations;
    private readonly ServiceContract.Interfaces.IOrderPaymentGateway _gateway;
    public OrderController(ShopDbContext db, CustomerSession session, OrderReservations reservations, ServiceContract.Interfaces.IOrderPaymentGateway gateway)
    { _db = db; _session = session; _reservations = reservations; _gateway = gateway; }
    private string Key => _session.GetOrCreate(HttpContext);

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
        var shipping = 0m;
        return Ok(new { coupon!.Code, Discount = discount, Shipping = shipping, Total = subtotal + shipping - discount });
    }

    [HttpPost("checkout")]
    public async Task<IActionResult> Checkout(CheckoutRequest request)
    {
        if (request.CheckoutToken == Guid.Empty) return BadRequest("شناسهٔ ثبت سفارش معتبر نیست.");
        if (request.ShippingCost != 0) return BadRequest("هزینهٔ ارسال در محل دریافت می‌شود و به مبلغ سفارش اضافه نمی‌شود.");
        if (request.FirstName?.Length > 100 || request.LastName?.Length > 100 || request.Phone?.Length > 20)
            return BadRequest("اطلاعات تماس معتبر نیست.");
        if (string.IsNullOrWhiteSpace(request.FirstName) || string.IsNullOrWhiteSpace(request.LastName) || string.IsNullOrWhiteSpace(request.Phone))
            return BadRequest("لطفاً نام، نام خانوادگی و شماره تماس را وارد کنید.");
        var hasCoupon = !string.IsNullOrWhiteSpace(request.CouponCode);
        var userId = 0;
        if (hasCoupon && (!int.TryParse(User.FindFirstValue(ClaimTypes.NameIdentifier), out userId)
            || Key != $"user-{userId}" || !await _db.Users.AnyAsync(x => x.Id == userId && x.IsActive)))
            return BadRequest("برای استفاده از کوپن باید با حساب فعال خود وارد شوید.");
        await _reservations.ExpireAsync(Key);
        await using var tx = _db.Database.CurrentTransaction is null ? await _db.Database.BeginTransactionAsync() : null;
        await PurchaseLimitPolicy.LockCustomerAsync(_db, Key);
        var previous = await _db.Orders.AsNoTracking().SingleOrDefaultAsync(x => x.CustomerKey == Key && x.CheckoutToken == request.CheckoutToken);
        if (previous is not null) return Ok(CheckoutResponse(previous));
        var address = await _db.Addresses.AsNoTracking().SingleOrDefaultAsync(x => x.Id == request.AddressId && x.CustomerKey == Key);
        if (address is null) return BadRequest("یک آدرس متعلق به خودتان برای سفارش انتخاب کنید.");
        if (string.IsNullOrWhiteSpace(address.Province) || string.IsNullOrWhiteSpace(address.City) || string.IsNullOrWhiteSpace(address.Details) || string.IsNullOrWhiteSpace(address.ReceiverName) || string.IsNullOrWhiteSpace(address.Phone) || address.Province.Length > 100 || address.City.Length > 100 || address.Details.Length > 1000 || address.PostalCode.Length != 10 || address.ReceiverName.Length > 200 || address.Phone.Length > 20)
            return BadRequest("اطلاعات آدرس کامل و معتبر نیست.");
        var cart = await _db.CartItems.Include(x => x.Product).Include(x => x.ProductVariant)
            .Where(x => x.CustomerKey == Key).ToListAsync();
        if (cart.Count == 0) return BadRequest("سبد خرید خالی است.");
        var order = new Order
        {
            CustomerKey = Key, Status = OrderStatus.Pending, CustomerFirstName = request.FirstName.Trim(), CustomerLastName = request.LastName.Trim(), CustomerPhone = request.Phone.Trim(),
            ShippingProvince = address.Province, ShippingCity = address.City, ShippingAddress = address.Details,
            ShippingPostalCode = address.PostalCode, ShippingReceiver = address.ReceiverName, ShippingPhone = address.Phone,
            ShippingMethod = ShippingPolicy.Method(address.Province), ShippingPayOnDelivery = true, ShippingCost = 0,
            CheckoutToken = request.CheckoutToken, ReservationExpiresAt = DateTime.UtcNow.AddMinutes(30)
        };
        foreach (var c in cart.OrderBy(x => x.ProductVariantId))
        {
            var limitError = await PurchaseLimitPolicy.ValidateAsync(_db, User, c.ProductVariant, c.Quantity);
            if (limitError is not null) return BadRequest($"«{c.Product.Name}»: {limitError}");
            var available = c.ProductVariant.StockQuantity - c.ProductVariant.ReservedQuantity;
            if (available < c.Quantity) return BadRequest($"موجودی «{c.Product.Name}» کافی نیست.");
            var unitPrice = CartPricing.GetFinalPrice(c.Product, c.ProductVariant, DateTime.UtcNow);
            var total = unitPrice * c.Quantity;
            order.Items.Add(new OrderItem { ProductId = c.ProductId, ProductVariantId = c.ProductVariantId, ProductName = c.Product.Name, UnitPrice = unitPrice, Quantity = c.Quantity, LineTotal = total });
            order.Subtotal += total;
            if (c.Quantity <= 0 || !c.Product.IsActive) return BadRequest("محصول انتخاب‌شده قابل خرید نیست.");
            var reserved = await _db.ProductVariants.Where(x => x.Id == c.ProductVariantId && x.StockQuantity - x.ReservedQuantity >= c.Quantity)
                .ExecuteUpdateAsync(setters => setters.SetProperty(x => x.ReservedQuantity, x => x.ReservedQuantity + c.Quantity));
            if (reserved != 1) return Conflict("موجودی محصول تغییر کرده است؛ سبد خرید را بررسی کنید.");
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
        var shipping = 0m;
        order.CouponDiscount = discount;
        order.Total = order.Subtotal + shipping - discount;
        _db.Orders.Add(order); _db.CartItems.RemoveRange(cart);
        if (coupon is not null)
            _db.CouponRedemptions.Add(new CouponRedemption { CouponId = coupon.Id, UserId = userId,
                Order = order, DiscountAmount = discount });
        try { await _db.SaveChangesAsync(); if (tx is not null) await tx.CommitAsync(); }
        catch (DbUpdateException ex) when (ex.InnerException is SqlException { Number: 2601 or 2627 } && coupon is not null)
        {
            return Conflict("این کد قبلاً برای حساب شما استفاده شده است.");
        }
        return Ok(CheckoutResponse(order));
    }

    // The legacy endpoint cannot mark an order paid. All payments go through server verification.
    [HttpPost("{id}/pay")]
    public IActionResult Pay(int id) => StatusCode(503, new { message = "برای پرداخت از مسیر امن درگاه استفاده کنید؛ پرداخت آنلاین هنوز فعال نیست." });

    private object CheckoutResponse(Order order) => new { order.Id, order.Total, order.Status, order.CouponDiscount,
        Shipping = order.ShippingCost, order.ShippingPayOnDelivery, order.ShippingMethod, order.ReservationExpiresAt,
        PaymentAvailable = _gateway.IsConfigured, message = "سفارش ثبت شد؛ هزینهٔ ارسال در محل دریافت می‌شود. ثبت سفارش به معنی پرداخت مبلغ کالا نیست." };

    private async Task<(DiscountCoupon? Coupon, string? Error, decimal Discount)> ValidateCoupon(
        string? rawCode, int userId, decimal subtotal, DateTime now)
    {
        var code = rawCode?.Trim().ToUpperInvariant();
        if (string.IsNullOrWhiteSpace(code)) return (null, "کد تخفیف را وارد کنید.", 0);
        var coupon = await _db.DiscountCoupons.AsNoTracking().SingleOrDefaultAsync(x => x.Code == code);
        if (coupon is null || !coupon.IsActive || coupon.ExpiresAt <= now)
            return (null, "کد تخفیف معتبر یا فعال نیست.", 0);
        if (coupon.Kind == CouponKind.FreeShipping) return (null, "با توجه به پرداخت کرایه در محل، کوپن ارسال رایگان قابل استفاده نیست.", 0);
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

public record CheckoutRequest(decimal ShippingCost, string FirstName = "", string LastName = "", string Phone = "", string? CouponCode = null, int AddressId = 0, Guid CheckoutToken = default);
public record CouponPreviewRequest(string Code);
