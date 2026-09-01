using Entities;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace lern.Controller;

[ApiController, Route("api/cart")]
public class CartController : ControllerBase
{
    private readonly ShopDbContext _db;
    public CartController(ShopDbContext db) => _db = db;
    private string Key => Request.Cookies["customer-key"] ?? "guest";

    [HttpGet]
    public async Task<IActionResult> Get()
    {
        EnsureCookie();
        return Ok(await _db.CartItems.AsNoTracking()
        .Include(x => x.Product).Include(x => x.ProductVariant)
        .Where(x => x.CustomerKey == Key).Select(x => new { x.Id, x.ProductId, x.ProductVariantId, ProductName = x.Product.Name, x.Quantity, x.ProductVariant.Price }).ToListAsync());
    }

    [HttpPost]
    public async Task<IActionResult> Add(AddCartItemRequest request)
    {
        EnsureCookie();
        var variant = await _db.ProductVariants.Include(x => x.ProductSaleOption).ThenInclude(x => x.Product)
            .SingleOrDefaultAsync(x => x.Id == request.ProductVariantId);
        if (variant is null || variant.AvailableQuantity < request.Quantity || request.Quantity <= 0) return BadRequest("تنوع محصول موجود نیست.");
        var item = await _db.CartItems.SingleOrDefaultAsync(x => x.CustomerKey == Key && x.ProductVariantId == request.ProductVariantId);
        if (item is null) _db.CartItems.Add(new CartItem { CustomerKey = Key, ProductId = variant.ProductSaleOption.ProductId, ProductVariantId = variant.Id, Quantity = request.Quantity });
        else item.Quantity = Math.Min(item.Quantity + request.Quantity, variant.AvailableQuantity);
        await _db.SaveChangesAsync();
        return Ok();
    }

    [HttpPut("{id}")]
    public async Task<IActionResult> Update(int id, UpdateCartItemRequest request)
    {
        var item = await _db.CartItems.SingleOrDefaultAsync(x => x.Id == id && x.CustomerKey == Key);
        if (item is null) return NotFound();
        item.Quantity = Math.Max(1, request.Quantity); item.UpdatedAt = DateTime.UtcNow;
        await _db.SaveChangesAsync(); return Ok();
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> Remove(int id)
    {
        var item = await _db.CartItems.SingleOrDefaultAsync(x => x.Id == id && x.CustomerKey == Key);
        if (item is null) return NotFound();
        _db.CartItems.Remove(item); await _db.SaveChangesAsync(); return NoContent();
    }
    private void EnsureCookie()
    {
        if (Request.Cookies["customer-key"] is null)
            Response.Cookies.Append("customer-key", Guid.NewGuid().ToString("N"), new CookieOptions { HttpOnly = true, IsEssential = true, SameSite = SameSiteMode.Lax, Expires = DateTimeOffset.UtcNow.AddYears(1) });
    }
}
public record AddCartItemRequest(int ProductVariantId, int Quantity);
public record UpdateCartItemRequest(int Quantity);
