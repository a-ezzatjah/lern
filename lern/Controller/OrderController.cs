using Entities;
using lern.Infrastructure;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

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

    [HttpPost("checkout")]
    public async Task<IActionResult> Checkout(CheckoutRequest request)
    {
        if (Request.Cookies["customer-key"] is null) return BadRequest("شناسه مشتری وجود ندارد؛ ابتدا سبد خرید را دریافت کنید.");
        if (request.ShippingCost != 300000)
            return BadRequest("هزینه ارسال انتخاب‌شده معتبر نیست.");
        if (string.IsNullOrWhiteSpace(request.FirstName) || string.IsNullOrWhiteSpace(request.LastName) || string.IsNullOrWhiteSpace(request.Phone))
            return BadRequest("لطفاً نام، نام خانوادگی و شماره تماس را وارد کنید.");
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
        order.Total = order.Subtotal + request.ShippingCost;
        _db.Orders.Add(order); _db.CartItems.RemoveRange(cart);
        await _db.SaveChangesAsync(); await tx.CommitAsync();
        return Ok(new { order.Id, order.Total, order.Status });
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
}

public record CheckoutRequest(decimal ShippingCost, string FirstName = "", string LastName = "", string Phone = "");
