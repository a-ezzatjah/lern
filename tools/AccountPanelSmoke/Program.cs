using System.Security.Claims;
using System.Text.Json;
using Entities;
using lern.Controller;
using lern.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

// Account fixtures are rolled back. Address API tests commit only uniquely named test addresses,
// then remove those addresses in finally because the API manages its own transactions.
using var settings = JsonDocument.Parse(await File.ReadAllTextAsync("lern/appsettings.json"));
var connection = settings.RootElement.GetProperty("ConnectionStrings").GetProperty("DefaultConnectionstring").GetString();
var options = new DbContextOptionsBuilder<ShopDbContext>().UseSqlServer(connection).Options;
void Check(bool passed, string message) { if (!passed) throw new Exception(message); Console.WriteLine("PASS " + message); }
await using (var db = new ShopDbContext(options))
{
    await using var transaction = await db.Database.BeginTransactionAsync();
    try
    {
        var tag = Guid.NewGuid().ToString("N");
        var user = new CustomerUser { PhoneNumber = tag[..11], FirstName = "Smoke", LastName = "Test", PasswordHash = "test", PasswordSalt = "test" };
        db.Users.Add(user); await db.SaveChangesAsync();
        var key = $"user-{user.Id}";
        var productId = await db.Products.Select(p => p.Id).FirstAsync();
        var now = DateTime.UtcNow;
        for (var i = 0; i < 23; i++)
        {
            db.Orders.Add(new Order { CustomerKey = key, CustomerFirstName = "Test", CustomerLastName = "Test", CustomerPhone = "09123456789", Total = 100, CreatedAt = now.AddDays(-400).AddMinutes(i) });
            db.ProductComments.Add(new ProductComment { CustomerKey = key, ProductId = productId, AuthorName = "Test", Body = "Test", CreatedAt = now.AddMinutes(i) });
        }
        var stranger = new Order { CustomerKey = "smoke-other-" + tag, CustomerFirstName = "Test", CustomerLastName = "Test", CustomerPhone = "09123456789" };
        db.Orders.Add(stranger);
        await db.SaveChangesAsync();
        var ownOrders = await db.Orders.Where(o => o.CustomerKey == key).OrderByDescending(o => o.CreatedAt).ThenByDescending(o => o.Id).Select(o => o.Id).ToListAsync();
        for (var i = 0; i < 33; i++)
        {
            var coupon = new DiscountCoupon { Code = tag[..16] + i, Title = "Smoke", RecipientUserId = user.Id, Kind = CouponKind.FixedAmount, Value = 10, ExpiresAt = i < 11 ? now.AddDays(10) : now.AddDays(-10) };
            db.DiscountCoupons.Add(coupon);
            if (i >= 22) db.CouponRedemptions.Add(new CouponRedemption { Coupon = coupon, UserId = user.Id, OrderId = ownOrders[i - 22] });
        }
        await db.SaveChangesAsync();
        var account = new AccountController(db, null!, null!) { ControllerContext = new ControllerContext { HttpContext = new DefaultHttpContext { User = new ClaimsPrincipal(new ClaimsIdentity(new[] { new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()) }, "test")) } } };
        async Task<AccountOrdersViewModel> Orders(int page = 1, string? search = null, OrderStatus? status = null, string? period = null, string? amount = null) => (AccountOrdersViewModel)((ViewResult)await account.Orders(status, period, amount, search, page)).Model!;
        var first = await Orders(-1); var second = await Orders(2); var last = await Orders(int.MaxValue);
        Check(first.Page == 1 && first.TotalCount == 23 && first.Orders.Select(o => o.Id).SequenceEqual(ownOrders.Take(10)), "orders first page: 10, customer isolation, negative page");
        Check(second.Orders.Select(o => o.Id).SequenceEqual(ownOrders.Skip(10).Take(10)) && last.Page == 3 && last.Orders.Count == 3, "orders second and last pages");
        var oldest = ownOrders[^1];
        var persian = string.Concat(oldest.ToString().Select(c => (char)('۰' + c - '0')));
        var searched = await Orders(int.MaxValue, "#" + persian, OrderStatus.Completed, "7days", "more5000");
        Check(searched.TotalCount == 1 && searched.Page == 1 && searched.Orders[0].Id == oldest, "Persian order number searches all pages and ignores conflicting filters");
        Check((await Orders(search: stranger.Id.ToString())).TotalCount == 0 && (await Orders(search: "invalid")).TotalCount == 0, "search hides another customer's order and handles invalid number");
        var commentFirst = (AccountCommentsViewModel)((ViewResult)await account.Comments(null, null, null, null, -1)).Model!;
        var commentSecond = (AccountCommentsViewModel)((ViewResult)await account.Comments(null, null, null, null, 2)).Model!;
        var commentLast = (AccountCommentsViewModel)((ViewResult)await account.Comments(null, null, null, null, int.MaxValue)).Model!;
        Check(commentFirst.Comments.Count == 10 && commentSecond.Comments.Count == 10 && commentLast.Comments.Count == 3 && commentLast.Page == 3 && !commentFirst.Comments.Select(c => c.Id).Intersect(commentSecond.Comments.Select(c => c.Id)).Any(), "comments: 10/10/3, no duplicates, page bounds");
        foreach (var tab in new[] { "active", "used", "expired" })
        {
            var filtered = db.DiscountCoupons.Where(c => c.RecipientUserId == null || c.RecipientUserId == user.Id);
            var used = db.CouponRedemptions.Where(r => r.UserId == user.Id).Select(r => r.CouponId);
            filtered = tab switch { "used" => filtered.Where(c => used.Contains(c.Id)), "expired" => filtered.Where(c => !used.Contains(c.Id) && (!c.IsActive || c.ExpiresAt <= now)), _ => filtered.Where(c => !used.Contains(c.Id) && c.IsActive && c.ExpiresAt > now) };
            var ids = await filtered.OrderByDescending(c => c.CreatedAt).ThenByDescending(c => c.Id).Select(c => c.Id).ToListAsync();
            var result = (AccountDiscountsViewModel)((ViewResult)await account.Discounts(tab, 2)).Model!;
            Check(result.Page == 2 && result.TotalCount == ids.Count && result.Coupons.Select(c => c.Id).SequenceEqual(ids.Skip(10).Take(10)), $"discounts {tab}: correct classification before pagination");
        }
    }
    finally { await transaction.RollbackAsync(); }
}
var addressKey = "smoke-address-" + Guid.NewGuid().ToString("N");
await using var addresses = new ShopDbContext(options);
try
{
    for (var i = 0; i < 9; i++) addresses.Addresses.Add(new Address { CustomerKey = addressKey, Title = "Test " + i, Province = "Test", City = "Test", Details = "Test", PostalCode = "1234567890", Phone = "09123456789", ReceiverName = "Test", IsDefault = i == 0 });
    await addresses.SaveChangesAsync();
    async Task<IActionResult> Save(bool create, int? id = null, string title = "Test", bool isDefault = false)
    {
        await using var db = new ShopDbContext(options);
        var context = new DefaultHttpContext();
        context.Request.Headers.Cookie = "customer-key=" + addressKey;
        if (id.HasValue) context.Request.QueryString = new QueryString("?id=" + id);
        var api = new AddressApiController(db) { ControllerContext = new ControllerContext { HttpContext = context } };
        var request = new AddressRequest(title, "Test", "Test", "Test", "1234567890", "09123456789", "Test", isDefault);
        return create ? await api.Create(request) : await api.Save(request);
    }
    Check(await Save(true) is OkObjectResult && await addresses.Addresses.CountAsync(a => a.CustomerKey == addressKey) == 10, "10th address allowed");
    Check(await Save(true, isDefault: true) is BadRequestObjectResult && await addresses.Addresses.CountAsync(a => a.CustomerKey == addressKey) == 10, "11th address blocked");
    var defaultId = await addresses.Addresses.Where(a => a.CustomerKey == addressKey && a.IsDefault).Select(a => a.Id).SingleAsync();
    Check(await Save(false, defaultId, "Edited") is OkObjectResult && await addresses.Addresses.AsNoTracking().AnyAsync(a => a.Id == defaultId && a.Title == "Edited" && !a.IsDefault), "editing allowed at address limit");
    var removed = await addresses.Addresses.Where(a => a.CustomerKey == addressKey && a.Id != defaultId).Select(a => a.Id).FirstAsync();
    await addresses.Addresses.Where(a => a.Id == removed && a.CustomerKey == addressKey).ExecuteDeleteAsync();
    Check(await Save(true) is OkObjectResult && await addresses.Addresses.CountAsync(a => a.CustomerKey == addressKey) == 10, "deletion releases one address slot");
}
finally { await addresses.Addresses.Where(a => a.CustomerKey == addressKey).ExecuteDeleteAsync(); }
Console.WriteLine("PASS all account panel checks; test data removed.");
