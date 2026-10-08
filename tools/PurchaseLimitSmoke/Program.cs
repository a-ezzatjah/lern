using System.ComponentModel.DataAnnotations;
using System.Security.Claims;
using System.Text.Json;
using AutoMapper;
using Entities;
using lern.Controller;
using lern.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Microsoft.AspNetCore.Mvc.Routing;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging.Abstractions;
using Service.Mapping;
using Service.Service;

// API methods own transactions. Fixtures use unique accounts/product and are removed in finally.
using var settings = JsonDocument.Parse(await File.ReadAllTextAsync("lern/appsettings.json"));
var connection = settings.RootElement.GetProperty("ConnectionStrings").GetProperty("DefaultConnectionstring").GetString();
var options = new DbContextOptionsBuilder<ShopDbContext>().UseSqlServer(connection).Options;
void Check(bool passed, string message) { if (!passed) throw new Exception(message); Console.WriteLine("PASS " + message); }
await using var fixtures = new ShopDbContext(options);
var pendingMigrations = (await fixtures.Database.GetPendingMigrationsAsync()).ToArray();
if (args.Contains("--apply-limit-migration") && pendingMigrations.Length > 0)
{
    Check(pendingMigrations.Length == 1 && pendingMigrations[0] == "20261008170015_AddVariantPurchaseLimit", "only purchase-limit migration is pending");
    await fixtures.GetService<IMigrator>().MigrateAsync(pendingMigrations[0]);
    Console.WriteLine("Applied nullable variant purchase-limit column.");
}
else if (pendingMigrations.Length > 0) throw new Exception("Apply the purchase-limit migration before running this check.");
var tag = "lim-" + Guid.NewGuid().ToString("N")[..8];
var guestKey = tag + "-guest";
var spoofKey = tag + "-spoof";
var userIds = new List<int>();
var productId = 0;
var categoryId = 0;
DefaultHttpContext Context(int? userId, string? cookie = null)
{
    var context = new DefaultHttpContext();
    if (userId.HasValue) context.User = new ClaimsPrincipal(new ClaimsIdentity(new[] { new Claim(ClaimTypes.NameIdentifier, userId.Value.ToString()) }, "test"));
    context.Request.Headers.Cookie = "customer-key=" + (cookie ?? (userId.HasValue ? $"user-{userId}" : guestKey));
    return context;
}
async Task<IActionResult> Cart(int? userId, int variantId, int quantity, int? cartId = null, string? cookie = null)
{
    await using var db = new ShopDbContext(options);
    var controller = new CartController(db) { ControllerContext = new ControllerContext { HttpContext = Context(userId, cookie) } };
    return cartId.HasValue ? await controller.Update(cartId.Value, new UpdateCartItemRequest(quantity)) : await controller.Add(new AddCartItemRequest(variantId, quantity));
}
async Task<IActionResult> Checkout(int userId)
{
    await using var db = new ShopDbContext(options);
    var controller = new OrderController(db) { ControllerContext = new ControllerContext { HttpContext = Context(userId, spoofKey) } };
    return await controller.Checkout(new CheckoutRequest(300000, "Test", "Test", "09123456789"));
}
async Task<IActionResult> Pay(int userId, int id)
{
    await using var db = new ShopDbContext(options);
    var controller = new OrderController(db) { ControllerContext = new ControllerContext { HttpContext = Context(userId) } };
    return await controller.Pay(id);
}
try
{
    var invalidInput = new AdminVariantInputViewModel { MaxPurchaseQuantityPerUser = 0 };
    Check(!Validator.TryValidateObject(invalidInput, new ValidationContext(invalidInput), [], true), "zero purchase limit is invalid");
    foreach (var suffix in new[] { "a", "b" })
    {
        var user = new CustomerUser { FirstName = "Smoke", LastName = "Test", PhoneNumber = tag[..9] + suffix, PasswordHash = "test", PasswordSalt = "test" };
        fixtures.Users.Add(user); await fixtures.SaveChangesAsync(); userIds.Add(user.Id);
    }
    var category = new Category { Name = tag, Slug = tag };
    fixtures.Categories.Add(category); await fixtures.SaveChangesAsync(); categoryId = category.Id;
    var model = new AdminProductCreateViewModel { Name = tag, Slug = tag, CategoryIds = [categoryId], Variants = [
        new() { SaleTitle = "Meter", SaleType = 2, Price = 1000, StockQuantity = 1000, MaxPurchaseQuantityPerUser = 10 },
        new() { SaleTitle = "Pack", SaleType = 1, Price = 2000, StockQuantity = 1000, MaxPurchaseQuantityPerUser = 3 },
        new() { SaleTitle = "Unlimited", SaleType = 4, Price = 3000, StockQuantity = 1000 }
    ] };
    using var cache = new MemoryCache(new MemoryCacheOptions());
    AdminController Admin(ShopDbContext db)
    {
        var context = Context(userIds[0]); context.Request.Headers["X-Requested-With"] = "XMLHttpRequest";
        return new AdminController(db, null!, NullLogger<AdminController>.Instance, cache) { ControllerContext = new ControllerContext { HttpContext = context }, TempData = new TempDataDictionary(context, new TestTempDataProvider()), Url = new TestUrlHelper() };
    }
    await using (var db = new ShopDbContext(options))
    {
        var result = await Admin(db).CreateProduct(model);
        productId = await db.Products.Where(p => p.Slug == tag).Select(p => p.Id).SingleAsync();
        Check(result is JsonResult, "admin create stores per-variant purchase limits");
    }
    var variants = await fixtures.ProductVariants.AsNoTracking().Where(v => v.ProductSaleOption.ProductId == productId).OrderBy(v => v.Id).ToListAsync();
    var meter = variants.Single(v => v.MaxPurchaseQuantityPerUser == 10);
    var pack = variants.Single(v => v.MaxPurchaseQuantityPerUser == 3);
    var unlimited = variants.Single(v => v.MaxPurchaseQuantityPerUser == null);
    await using (var db = new ShopDbContext(options))
    {
        var editor = Admin(db);
        var editModel = (AdminProductCreateViewModel)((ViewResult)await editor.EditProductPage(productId)).Model!;
        Check(editModel.Variants.Single(v => v.SaleType == 2).MaxPurchaseQuantityPerUser == 10, "edit form loads purchase limit");
        editModel.Variants.Single(v => v.SaleType == 2).MaxPurchaseQuantityPerUser = 12;
        Check(await editor.EditProduct(productId, editModel) is JsonResult, "admin edit saves purchase limit");
        Check(await db.ProductVariants.AsNoTracking().AnyAsync(v => v.Id == meter.Id && v.MaxPurchaseQuantityPerUser == 12), "variant identity and purchase history are preserved on edit");
    }
    await fixtures.ProductVariants.Where(v => v.Id == meter.Id).ExecuteUpdateAsync(s => s.SetProperty(v => v.MaxPurchaseQuantityPerUser, 10));
    var a = userIds[0]; var b = userIds[1];
    foreach (var sample in new[] { (User: a, Quantity: 6, Status: OrderStatus.Completed), (User: a, Quantity: 100, Status: OrderStatus.Cancelled), (User: b, Quantity: 2, Status: OrderStatus.Paid) })
    {
        fixtures.Orders.Add(new Order { CustomerKey = $"user-{sample.User}", CustomerFirstName = "Test", CustomerLastName = "Test", CustomerPhone = "09123456789", Status = sample.Status, Items = [new OrderItem { ProductId = productId, ProductVariantId = meter.Id, ProductName = tag, Quantity = sample.Quantity, UnitPrice = 1000, LineTotal = 1000 * sample.Quantity }] });
    }
    await fixtures.SaveChangesAsync();
    {
        var mapping = new MapperConfiguration(cfg => cfg.AddProfile<ProductMappingProfile>(), NullLoggerFactory.Instance);
        var productService = new ProductService(fixtures, mapping.CreateMapper(), null!, null!);
        var productController = new StoreProductController(productService, fixtures)
            { ControllerContext = new ControllerContext { HttpContext = Context(a) } };
        var page = (StoreProductDetailsViewModel)((ViewResult)await productController.Details(productId)).Model!;
        var choice = page.Options.SelectMany(o => o.Choices).Single(c => c.Id == meter.Id);
        Check(choice.MaxPurchaseQuantityPerUser == 10 && choice.RemainingPurchaseQuantity == 4 && choice.PurchaseQuantityLimit == 4,
            "product page displays lifetime limit and remaining four-unit quota");
        productController.ControllerContext = new ControllerContext { HttpContext = Context(null) };
        var guestPage = (StoreProductDetailsViewModel)((ViewResult)await productController.Details(productId)).Model!;
        Check(guestPage.Options.SelectMany(o => o.Choices).Single(c => c.Id == meter.Id).PurchaseQuantityLimit == 0,
            "limited variant prompts guests to sign in without marking stock unavailable");
    }
    Check(await Cart(a, meter.Id, 5) is BadRequestObjectResult, "previous six purchases leave four, cancelled purchases excluded");
    Check(await Cart(a, meter.Id, 4, cookie: spoofKey) is OkResult, "remaining four allowed using trusted account despite spoofed cookie");
    Check(await Cart(a, meter.Id, 1) is BadRequestObjectResult, "repeated cart additions cannot exceed lifetime limit");
    var cartId = await fixtures.CartItems.Where(c => c.CustomerKey == $"user-{a}" && c.ProductVariantId == meter.Id).Select(c => c.Id).SingleAsync();
    Check(await Cart(a, meter.Id, 5, cartId) is BadRequestObjectResult && await Cart(a, meter.Id, 3, cartId) is OkResult && await Cart(a, meter.Id, 4, cartId) is OkResult, "cart update enforces remaining lifetime quota");
    Check(await Cart(null, meter.Id, 1, cookie: $"user-{a}") is BadRequestObjectResult, "guest cannot spoof account to buy limited variant");
    Check(await Cart(null, unlimited.Id, 20) is OkResult, "unlimited variant remains available to guests");
    Check(await Cart(b, meter.Id, 8) is OkResult && await Cart(b, pack.Id, 3) is OkResult && await Cart(b, pack.Id, 1) is BadRequestObjectResult, "limits are independent by user and variant");
    await fixtures.ProductVariants.Where(v => v.Id == meter.Id).ExecuteUpdateAsync(s => s.SetProperty(v => v.MaxPurchaseQuantityPerUser, 8));
    Check(await Checkout(a) is BadRequestObjectResult && await fixtures.CartItems.AnyAsync(c => c.Id == cartId), "checkout rechecks limit changed after cart entry and retains cart");
    await fixtures.ProductVariants.Where(v => v.Id == meter.Id).ExecuteUpdateAsync(s => s.SetProperty(v => v.MaxPurchaseQuantityPerUser, 10));
    Check(await Checkout(a) is OkObjectResult, "checkout accepts six previous plus four new");
    var pendingId = await fixtures.Orders.Where(o => o.CustomerKey == $"user-{a}" && o.Status == OrderStatus.Pending).Select(o => o.Id).SingleAsync();
    Check(await Cart(a, meter.Id, 1) is BadRequestObjectResult, "pending order consumes quota across subsequent orders");
    await fixtures.Orders.Where(o => o.Id == pendingId).ExecuteUpdateAsync(s => s.SetProperty(o => o.Status, OrderStatus.Cancelled));
    Check(await Pay(a, pendingId) is ConflictObjectResult, "cancelled order cannot be paid to bypass purchase limit");
    var concurrentAdds = await Task.WhenAll(Cart(a, meter.Id, 3), Cart(a, meter.Id, 3));
    Check(concurrentAdds.Count(r => r is OkResult) == 1 && concurrentAdds.Count(r => r is BadRequestObjectResult) == 1, "concurrent cart requests share the four-unit quota");
    var newCartId = await fixtures.CartItems.Where(c => c.CustomerKey == $"user-{a}" && c.ProductVariantId == meter.Id).Select(c => c.Id).SingleAsync();
    Check(await Cart(a, meter.Id, 4, newCartId) is OkResult, "cancelled order releases quota");
    var concurrentCheckouts = await Task.WhenAll(Checkout(a), Checkout(a));
    Check(concurrentCheckouts.Count(r => r is OkObjectResult) == 1 && concurrentCheckouts.Count(r => r is BadRequestObjectResult) == 1, "concurrent checkouts create only one order");
    var paidId = await fixtures.Orders.Where(o => o.CustomerKey == $"user-{a}" && o.Status == OrderStatus.Pending).Select(o => o.Id).SingleAsync();
    Check(await Pay(a, paidId) is OkObjectResult && await Pay(a, paidId) is OkObjectResult && await Cart(a, meter.Id, 1) is BadRequestObjectResult, "payment keeps lifetime quota and repeat payment is idempotent");
    var purchased = await fixtures.OrderItems.Where(i => i.ProductVariantId == meter.Id && i.Order.CustomerKey == $"user-{a}" && i.Order.Status != OrderStatus.Cancelled).SumAsync(i => i.Quantity);
    Check(purchased == 10 && !await fixtures.CartItems.AnyAsync(c => c.CustomerKey == spoofKey), "final purchased total is exactly ten and cookie cannot reset quota");
    Console.WriteLine("PASS lifetime purchase limit integration checks.");
}
finally
{
    // Only IDs and keys created by this invocation are deleted.
    var keys = userIds.Select(id => $"user-{id}").Append(guestKey).Append(spoofKey).ToArray();
    await using var cleanup = new ShopDbContext(options);
    if (productId == 0) productId = await cleanup.Products.Where(p => p.Slug == tag).Select(p => p.Id).SingleOrDefaultAsync();
    await using var cleanupTransaction = await cleanup.Database.BeginTransactionAsync();
    var orderIds = await cleanup.Orders.Where(o => keys.Contains(o.CustomerKey)).Select(o => o.Id).ToListAsync();
    await cleanup.PaymentTransactions.Where(p => orderIds.Contains(p.OrderId)).ExecuteDeleteAsync();
    await cleanup.OrderItems.Where(i => orderIds.Contains(i.OrderId)).ExecuteDeleteAsync();
    await cleanup.Orders.Where(o => orderIds.Contains(o.Id)).ExecuteDeleteAsync();
    await cleanup.CartItems.Where(c => keys.Contains(c.CustomerKey)).ExecuteDeleteAsync();
    if (productId > 0)
    {
        await cleanup.ProductViewHistories.Where(h => h.ProductId == productId).ExecuteDeleteAsync();
        await cleanup.ProductCategories.Where(c => c.ProductId == productId).ExecuteDeleteAsync();
        await cleanup.ProductVariants.Where(v => v.ProductSaleOption.ProductId == productId).ExecuteDeleteAsync();
        await cleanup.ProductSaleOptions.Where(o => o.ProductId == productId).ExecuteDeleteAsync();
        await cleanup.Products.Where(p => p.Id == productId).ExecuteDeleteAsync();
    }
    if (categoryId > 0) await cleanup.Categories.Where(c => c.Id == categoryId).ExecuteDeleteAsync();
    await cleanup.Users.Where(u => userIds.Contains(u.Id)).ExecuteDeleteAsync();
    await cleanupTransaction.CommitAsync();
    Console.WriteLine("Test fixtures removed.");
}

sealed class TestTempDataProvider : ITempDataProvider
{
    public IDictionary<string, object> LoadTempData(HttpContext context) => new Dictionary<string, object>();
    public void SaveTempData(HttpContext context, IDictionary<string, object> values) { }
}

sealed class TestUrlHelper : IUrlHelper
{
    public ActionContext ActionContext { get; } = new();
    public string? Action(UrlActionContext context) => "/Admin";
    public string? Content(string? contentPath) => contentPath;
    public bool IsLocalUrl(string? url) => url?.StartsWith('/') == true;
    public string? Link(string? routeName, object? values) => "/Admin";
    public string? RouteUrl(UrlRouteContext context) => "/Admin";
}
