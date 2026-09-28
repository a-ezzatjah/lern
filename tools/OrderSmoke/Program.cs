using System.Text.Json;
using Entities;
using lern.Controller;
using lern.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc.Abstractions;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.Mvc.ViewEngines;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.DependencyInjection;

// Read-only integration check: no startup, migrations, seed data, or writes.
using var settings = JsonDocument.Parse(await File.ReadAllTextAsync("lern/appsettings.json"));
var connection = settings.RootElement.GetProperty("ConnectionStrings").GetProperty("DefaultConnectionstring").GetString();
await using var db = new ShopDbContext(new DbContextOptionsBuilder<ShopDbContext>().UseSqlServer(connection).Options);
var controller = new AdminOrdersController(db);
async Task<AdminOrderListViewModel> List(string? search = null, OrderStatus? status = null, string? period = null, DateTime? from = null, DateTime? to = null, int page = 1)
    => (AdminOrderListViewModel)((ViewResult)await controller.Index(search, status, period, from, to, page)).Model!;
void Check(bool condition, string message) { if (!condition) throw new Exception(message); Console.WriteLine("PASS " + message); }

var expected = await db.Orders.AsNoTracking().OrderByDescending(x => x.CreatedAt).ThenByDescending(x => x.Id).ToListAsync();
var first = await List(page: -1);
Check(first.TotalCount == expected.Count && first.Page == 1 && first.Orders.Select(x => x.Id).SequenceEqual(expected.Take(10).Select(x => x.Id)), "all customers, newest first, page bounds");
var last = await List(page: int.MaxValue);
Check(last.Page == last.TotalPages && last.Orders.Select(x => x.Id).SequenceEqual(expected.Skip((last.Page - 1) * 10).Select(x => x.Id)), "last page");
foreach (var status in Enum.GetValues<OrderStatus>())
    Check((await List(status: status)).TotalCount == expected.Count(x => x.Status == status), "status " + status);
Check((await List(search: "no-such-order-9e62a" )).TotalCount == 0, "empty search results");
if (expected.FirstOrDefault() is { } sample)
{
    Check((await List(search: "#" + sample.Id)).Orders.Any(x => x.Id == sample.Id), "order number search");
    var localDate = AdminOrderDisplay.LocalTime(sample.CreatedAt).Date;
    Check((await List(period: "custom", from: localDate, to: localDate)).TotalCount == expected.Count(x => AdminOrderDisplay.LocalTime(x.CreatedAt).Date == localDate), "inclusive custom date in Tehran time");
    var details = ((AdminOrderDetailsViewModel)((ViewResult)await controller.Details(sample.Id)).Model!).Order;
    Check(details.Items.Count == await db.OrderItems.CountAsync(x => x.OrderId == sample.Id), "details load actual items");
    Check(details.Transactions.Count == await db.PaymentTransactions.CountAsync(x => x.OrderId == sample.Id), "details load actual payments");
}
Check(await controller.Details(-1) is NotFoundResult, "unknown order returns 404");
var authorization = (AuthorizeAttribute?)Attribute.GetCustomAttribute(typeof(AdminOrdersController), typeof(AuthorizeAttribute));
Check(authorization?.Roles == "Admin", "admin role required on both endpoints");
Check(((ViewResult)await controller.Index(null, null, null, null, null)).ViewName == "~/Views/Admin/Orders/Index.cshtml", "dedicated admin view");
Console.WriteLine($"Completed read-only checks against {expected.Count} orders.");

// Render compiled Razor templates with in-memory fixtures; nothing is inserted into SQL.
var builder = WebApplication.CreateBuilder(new WebApplicationOptions
{
    ApplicationName = typeof(AdminOrdersController).Assembly.GetName().Name,
    ContentRootPath = Path.GetFullPath("lern")
});
builder.Services.AddControllersWithViews().AddApplicationPart(typeof(AdminOrdersController).Assembly);
await using var app = builder.Build();
app.MapControllers();
app.Urls.Add("http://127.0.0.1:0");
await app.StartAsync();
using var scope = app.Services.CreateScope();
async Task<string> Render(string path, object model, string action)
{
    var http = new DefaultHttpContext { RequestServices = scope.ServiceProvider };
    http.Request.Scheme = "http";
    http.Request.Host = new HostString("localhost");
    http.SetEndpoint(new Endpoint(_ => Task.CompletedTask, new EndpointMetadataCollection(), "Razor smoke render"));
    var route = new RouteData();
    route.Values["controller"] = "AdminOrders";
    route.Values["action"] = action;
    http.Request.RouteValues = route.Values;
    var context = new ActionContext(http, route, new ActionDescriptor());
    var engine = scope.ServiceProvider.GetRequiredService<ICompositeViewEngine>();
    var view = engine.GetView(null, path, true);
    if (!view.Success) throw new Exception("View not found: " + path);
    using var writer = new StringWriter();
    var data = new ViewDataDictionary(new EmptyModelMetadataProvider(), new ModelStateDictionary()) { Model = model };
    var temp = new TempDataDictionary(http, scope.ServiceProvider.GetRequiredService<ITempDataProvider>());
    await view.View.RenderAsync(new ViewContext(context, view.View, data, temp, writer, new HtmlHelperOptions()));
    return writer.ToString();
}
var fixture = new Order
{
    Id = 123, CustomerFirstName = "<script>bad()</script>", CustomerLastName = "Test", CustomerPhone = "09000000000",
    Total = 500000, Subtotal = 200000, Status = OrderStatus.Paid, CreatedAt = DateTime.UtcNow,
    Items = new List<OrderItem> { new() { Id = 1, ProductName = "Sample", Quantity = 2, UnitPrice = 100000, LineTotal = 200000 } },
    Transactions = new List<PaymentTransaction> { new() { Id = 1, Gateway = "manual", Status = PaymentStatus.Successful, Amount = 500000 } }
};
var html = await Render("~/Views/Admin/Orders/Index.cshtml", new AdminOrderListViewModel { Orders = new() { fixture }, Page = 1, TotalCount = 1 }, "Index");
Check(html.Contains("/Admin/Orders/123") && !html.Contains("/account/orders", StringComparison.OrdinalIgnoreCase), "admin menu and details stay in admin routes");
Check(html.Contains("&lt;script&gt;") && !html.Contains("<script>bad()"), "customer text is HTML encoded");
Check(html.Contains("#123") && html.Contains("500,000"), "nonempty list renders real model fields");
var detailHtml = await Render("~/Views/Admin/Orders/Details.cshtml", new AdminOrderDetailsViewModel
{
    Order = fixture,
    CurrentAddress = new Address { Province = "تهران", City = "تهران", Details = "آدرس فعلی", ReceiverName = "گیرنده", PostalCode = "1234567890" },
    CustomerEmail = "customer@example.com"
}, "Details");
Check(detailHtml.Contains("Sample") && detailHtml.Contains("order-status") && detailHtml.Contains("window.print()"), "details, tracking anchor and print action render");
Check(detailHtml.Contains("آدرس فعلی") && detailHtml.Contains("customer@example.com") && detailHtml.Contains("500,000"), "customer, address and totals render from model");
Check(detailHtml.Contains("&lt;script&gt;") && !detailHtml.Contains("<script>bad()"), "order customer name is HTML encoded");
var emptyHtml = await Render("~/Views/Admin/Orders/Index.cshtml", new AdminOrderListViewModel { Page = 1 }, "Index");
Check(emptyHtml.Contains("colspan=\"7\""), "empty table keeps all seven columns");
await app.StopAsync();
