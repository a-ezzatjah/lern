using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using Entities;
using Microsoft.EntityFrameworkCore;

namespace lern.Infrastructure;

public static class PurchaseLimitPolicy
{
    public static string? AccountKey(ClaimsPrincipal user) =>
        user.Identity?.IsAuthenticated == true &&
        int.TryParse(user.FindFirstValue(ClaimTypes.NameIdentifier), out var id) && id > 0
            ? $"user-{id}" : null;

    // Serialize cart/order changes for one account, including requests from multiple devices.
    public static Task LockCustomerAsync(ShopDbContext db, string customerKey)
    {
        var resource = "purchase-limit:" + Convert.ToHexString(SHA256.HashData(Encoding.UTF8.GetBytes(customerKey)));
        return db.Database.ExecuteSqlInterpolatedAsync($@"
DECLARE @result int;
EXEC @result = sys.sp_getapplock @Resource = {resource}, @LockMode = 'Exclusive',
    @LockOwner = 'Transaction', @LockTimeout = 10000;
IF @result < 0 THROW 51000, 'Could not lock customer purchases.', 1;");
    }

    public static async Task<string?> ValidateAsync(ShopDbContext db, ClaimsPrincipal user,
        ProductVariant variant, long requestedQuantity, int? excludingOrderId = null)
    {
        if (requestedQuantity <= 0) return "تعداد خرید باید بیشتر از صفر باشد.";
        if (variant.MaxPurchaseQuantityPerUser is not int limit) return null;
        var key = AccountKey(user);
        if (key is null || !int.TryParse(user.FindFirstValue(ClaimTypes.NameIdentifier), out var userId) ||
            !await db.Users.AnyAsync(x => x.Id == userId && x.IsActive))
            return "برای خرید این حالت فروش که سقف خرید دارد، ابتدا وارد حساب کاربری خود شوید.";
        var purchased = await db.OrderItems
            .Where(x => x.ProductVariantId == variant.Id && x.Order.CustomerKey == key &&
                x.Order.Status != OrderStatus.Cancelled && (!excludingOrderId.HasValue || x.OrderId != excludingOrderId.Value))
            .SumAsync(x => (long?)x.Quantity) ?? 0;
        var remaining = Math.Max(0L, (long)limit - purchased);
        if (requestedQuantity <= remaining) return null;
        return $"سقف مجموع خرید شما از این حالت فروش {limit} واحد است. با احتساب سفارش‌های قبلی و در انتظار پرداخت، فقط {remaining} واحد دیگر می‌توانید بخرید.";
    }
}
