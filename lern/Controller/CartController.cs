using Entities;
using lern.Infrastructure;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace lern.Controller;

[ApiController, Route("api/cart")]
public class CartController : ControllerBase
{
    private readonly ShopDbContext _db;
    public CartController(ShopDbContext db) => _db = db;
    [HttpGet]
    public async Task<IActionResult> Get()
    {
        var customerKey = EnsureCustomerKey();
        var now = DateTime.UtcNow;
        var cartItems = await _db.CartItems.AsNoTracking()
            .Include(x => x.Product).ThenInclude(x => x.ProductImages)
            .Include(x => x.ProductVariant).ThenInclude(x => x.ProductImages)
            .Include(x => x.ProductVariant).ThenInclude(x => x.ProductSaleOption)
            .Include(x => x.ProductVariant).ThenInclude(x => x.saleoptioncolor)
            .Where(x => x.CustomerKey == customerKey)
            .ToListAsync();

        return Ok(cartItems.Select(x =>
        {
            var originalPrice = x.ProductVariant.Price;
            var price = CartPricing.GetFinalPrice(x.Product, x.ProductVariant, now);
            var discountAmount = originalPrice - price;
            var discountPercent = originalPrice == 0 ? 0 : Math.Round(discountAmount / originalPrice * 100m, 0);

            return new
            {
            x.Id,
            x.ProductId,
            x.ProductVariantId,
            ProductName = x.Product.Name,
            x.Quantity,
            OriginalPrice = originalPrice,
            Price = price,
            DiscountAmount = discountAmount,
            DiscountPercent = discountPercent,
            SaleOptionTitle = x.ProductVariant.ProductSaleOption.Title,
            Color = x.ProductVariant.saleoptioncolor == null ? null : x.ProductVariant.saleoptioncolor.Color,
            ImageUrl = x.ProductVariant.ProductImages
                .OrderByDescending(image => image.IsPrimary)
                .ThenBy(image => image.SortOrder)
                .Select(image => image.ImageUrl)
                .FirstOrDefault()
                ?? x.Product.ProductImages
                    .OrderByDescending(image => image.IsPrimary)
                    .ThenBy(image => image.SortOrder)
                    .Select(image => image.ImageUrl)
                    .FirstOrDefault()
            };
        }));
    }

    [HttpPost]
    public async Task<IActionResult> Add(AddCartItemRequest request)
    {
        var customerKey = EnsureCustomerKey();
        var variant = await _db.ProductVariants.Include(x => x.ProductSaleOption).ThenInclude(x => x.Product)
            .SingleOrDefaultAsync(x => x.Id == request.ProductVariantId);
        if (variant is null || request.Quantity <= 0)
            return BadRequest("تنوع محصول یا تعداد درخواستی معتبر نیست.");

        var item = await _db.CartItems.SingleOrDefaultAsync(x => x.CustomerKey == customerKey && x.ProductVariantId == request.ProductVariantId);
        var requestedQuantity = (item?.Quantity ?? 0) + request.Quantity;
        if (variant.AvailableQuantity < requestedQuantity)
            return BadRequest("موجودی محصول برای تعداد درخواستی کافی نیست.");

        if (item is null)
            _db.CartItems.Add(new CartItem
            {
                CustomerKey = customerKey,
                ProductId = variant.ProductSaleOption.ProductId,
                ProductVariantId = variant.Id,
                Quantity = request.Quantity
            });
        else
        {
            item.Quantity = requestedQuantity;
            item.UpdatedAt = DateTime.UtcNow;
        }

        await _db.SaveChangesAsync();
        return Ok();
    }

    [HttpPut("{id}")]
    public async Task<IActionResult> Update(int id, UpdateCartItemRequest request)
    {
        var customerKey = EnsureCustomerKey();
        var item = await _db.CartItems.SingleOrDefaultAsync(x => x.Id == id && x.CustomerKey == customerKey);
        if (item is null) return NotFound();
        if (request.Quantity <= 0)
            return BadRequest("تعداد باید بیشتر از صفر باشد.");

        var variant = await _db.ProductVariants.SingleOrDefaultAsync(x => x.Id == item.ProductVariantId);
        if (variant is null || variant.AvailableQuantity < request.Quantity)
            return BadRequest("موجودی محصول برای تعداد درخواستی کافی نیست.");

        item.Quantity = request.Quantity;
        item.UpdatedAt = DateTime.UtcNow;
        await _db.SaveChangesAsync();
        return Ok();
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> Remove(int id)
    {
        var customerKey = EnsureCustomerKey();
        var item = await _db.CartItems.SingleOrDefaultAsync(x => x.Id == id && x.CustomerKey == customerKey);
        if (item is null) return NotFound();
        _db.CartItems.Remove(item); await _db.SaveChangesAsync(); return NoContent();
    }
    private string EnsureCustomerKey()
    {
        var customerKey = Request.Cookies["customer-key"];
        if (!string.IsNullOrWhiteSpace(customerKey)) return customerKey;

        customerKey = Guid.NewGuid().ToString("N");
        Response.Cookies.Append("customer-key", customerKey, new CookieOptions { HttpOnly = true, IsEssential = true, SameSite = SameSiteMode.Lax, Expires = DateTimeOffset.UtcNow.AddYears(1) });
        return customerKey;
    }
}
public record AddCartItemRequest(int ProductVariantId, int Quantity);
public record UpdateCartItemRequest(int Quantity);
