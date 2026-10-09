using System.Text.Json;
using Entities;
using lern.Controller;
using lern.Infrastructure;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Service.Service;
using ServiceContract.Interfaces;

void Check(bool condition, string message) { if (!condition) throw new Exception(message); Console.WriteLine("PASS " + message); }
using var settings = JsonDocument.Parse(await File.ReadAllTextAsync("lern/appsettings.json"));
var connection = settings.RootElement.GetProperty("ConnectionStrings").GetProperty("DefaultConnectionstring").GetString();
await using var db = new ShopDbContext(new DbContextOptionsBuilder<ShopDbContext>().UseSqlServer(connection).Options);
await db.Database.MigrateAsync();
Check(!db.Database.HasPendingModelChanges(), "migration matches model");
await using var tx = await db.Database.BeginTransactionAsync();
var session = new CustomerSession(new EphemeralDataProtectionProvider());
DefaultHttpContext Context(string key) { var c = new DefaultHttpContext(); c.Request.Headers.Cookie = "customer-key=" + key; return c; }
var key = Guid.NewGuid().ToString("N");
Check(session.GetOrCreate(Context(key)) == key, "legacy anonymous GUID migrates to protected session");
Check(session.GetOrCreate(Context("user-1")) != "user-1", "anonymous cookie cannot impersonate an account");
var signedContext = Context(key); session.GetOrCreate(signedContext);
var signed = signedContext.Response.Headers.SetCookie.ToString().Split(';')[0];
var signedRequest = new DefaultHttpContext(); signedRequest.Request.Headers.Cookie = signed;
Check(session.GetOrCreate(signedRequest) == key, "protected guest session survives requests");
var reservations = new OrderReservations(db);
var disabled = new UnavailablePaymentGateway();
OrderController Controller(string customer) => new(db, session, reservations, disabled) { ControllerContext = new ControllerContext { HttpContext = Context(customer) } };
var product = await db.Products.FirstAsync(x => x.IsActive);
var option = await db.ProductSaleOptions.FirstAsync(x => x.ProductId == product.Id);
var variant = new ProductVariant { ProductSaleOptionId = option.Id, Sku = "CHECKOUT-TEST-" + key, Price = 100000, StockQuantity = 20 };
db.ProductVariants.Add(variant);
var address = new Address { CustomerKey = key, Title = "test", Province = "تهران", City = "تهران", Details = "آدرس آزمایشی", PostalCode = "1234567890", ReceiverName = "مشتری مهمان", Phone = "09120000000" };
db.Addresses.Add(address); await db.SaveChangesAsync();
async Task AddCart(string customer, int qty) { db.CartItems.Add(new CartItem { CustomerKey = customer, ProductId = product.Id, ProductVariantId = variant.Id, Quantity = qty }); await db.SaveChangesAsync(); }
await AddCart(key, 2);
var token = Guid.NewGuid();
var request = new CheckoutRequest(0, "مشتری", "مهمان", "09120000000", AddressId: address.Id, CheckoutToken: token);
Check(await Controller(key).Checkout(request with { ShippingCost = 1 }) is BadRequestObjectResult, "client cannot add shipping amount");
Check(await Controller(Guid.NewGuid().ToString("N")).Checkout(request) is BadRequestObjectResult, "another guest cannot use this address");
Check(await Controller(key).Checkout(request) is OkObjectResult, "guest checkout succeeds without login");
var order = await db.Orders.SingleAsync(x => x.CustomerKey == key && x.CheckoutToken == token);
Check(order.ShippingMethod == "courier" && order.ShippingPayOnDelivery && order.ShippingCost == 0, "Tehran province courier and collect shipping");
Check(order.Total == CartPricing.GetFinalPrice(product, variant, DateTime.UtcNow) * 2, "merchandise total is calculated on server");
Check(await Controller(key).Checkout(request) is OkObjectResult && await db.Orders.CountAsync(x => x.CheckoutToken == token) == 1, "retry does not duplicate checkout");
await db.Entry(variant).ReloadAsync();
Check(variant.ReservedQuantity == 2 && variant.StockQuantity == 20, "checkout reserves stock without pretending payment");
address.Details = "آدرس جدید"; await db.SaveChangesAsync();
Check(order.ShippingAddress == "آدرس آزمایشی", "order retains immutable address snapshot");
Check(Controller(key).Pay(order.Id) is ObjectResult { StatusCode: 503 }, "legacy fake payment is disabled");
Check(!(await new OrderPayments(db, disabled).StartAsync(order.Id, key, "https://store.test/payments/callback")).Succeeded, "missing gateway cannot take payment");
var fake = new TestGateway(); var payments = new OrderPayments(db, fake);
Check(!(await payments.VerifyAsync("unknown")).Succeeded, "unknown authority cannot pay an order");
Check((await payments.StartAsync(order.Id, key, "https://store.test/payments/callback")).Succeeded, "gateway start stores pending transaction");
Check(!(await payments.StartAsync(order.Id, key, "https://store.test/payments/callback")).Succeeded, "cannot start two pending payments");
Check((await payments.VerifyAsync(fake.Authority)).Succeeded, "server-verified payment succeeds");
await db.Entry(variant).ReloadAsync(); await db.Entry(order).ReloadAsync();
Check(order.Status == OrderStatus.Paid && variant.StockQuantity == 18 && variant.ReservedQuantity == 0, "verified payment consumes reserved stock once");
Check((await payments.VerifyAsync(fake.Authority)).Succeeded, "duplicate callback is idempotent");
await db.Entry(variant).ReloadAsync(); Check(variant.StockQuantity == 18, "duplicate callback never consumes stock twice");
var otherKey = Guid.NewGuid().ToString("N");
var other = new Address { CustomerKey = otherKey, Title = "test", Province = "اصفهان", City = "اصفهان", Details = "آدرس شهرستان", PostalCode = "1234567890", ReceiverName = "مهمان", Phone = "09120000000" };
db.Addresses.Add(other); await db.SaveChangesAsync(); await AddCart(otherKey, 3);
Check(await Controller(otherKey).Checkout(request with { AddressId = other.Id, CheckoutToken = Guid.NewGuid() }) is OkObjectResult, "outside Tehran guest checkout succeeds");
var freight = await db.Orders.SingleAsync(x => x.CustomerKey == otherKey);
Check(freight.ShippingMethod == "regional" && freight.ShippingPayOnDelivery, "other provinces await admin shipping choice with collect shipping");
var failedGateway = new TestGateway { Fail = true };
var failedFlow = new OrderPayments(db, failedGateway);
Check((await failedFlow.StartAsync(freight.Id, otherKey, "https://store.test/payments/callback")).Succeeded, "second order has independent payment");
Check(!(await failedFlow.VerifyAsync(failedGateway.Authority)).Succeeded, "gateway rejection cannot mark order paid");
Check(await db.PaymentTransactions.AnyAsync(x => x.OrderId == freight.Id && x.Status == PaymentStatus.Failed), "definitive gateway failure recorded");
db.ChangeTracker.Clear();
var retryGateway = new TestGateway(); var retryFlow = new OrderPayments(db, retryGateway);
Check((await retryFlow.StartAsync(freight.Id, otherKey, "https://store.test/payments/callback")).Succeeded, "failed payment permits a new attempt");
freight = await db.Orders.Include(x => x.Items).SingleAsync(x => x.Id == freight.Id);
freight.ReservationExpiresAt = DateTime.UtcNow.AddMinutes(-1); await db.SaveChangesAsync();
Check(await reservations.ExpireAsync(otherKey) == 1, "expired order is cancelled");
await db.Entry(variant).ReloadAsync(); Check(variant.ReservedQuantity == 0 && variant.StockQuantity == 18, "expiration releases only reservations");
Check(await reservations.ExpireAsync(otherKey) == 0, "expiration is idempotent");
Check((await retryFlow.VerifyAsync(retryGateway.Authority)).Succeeded, "late real payment remains recorded");
await db.Entry(freight).ReloadAsync(); await db.Entry(variant).ReloadAsync();
Check(freight.Status == OrderStatus.PaymentReview && variant.StockQuantity == 18 && variant.ReservedQuantity == 0, "late payment goes to review without consuming released inventory");
var receipt = new OrderReceiptController(db, session, reservations, disabled) { ControllerContext = new ControllerContext { HttpContext = Context(key) } };
Check(await receipt.Details(freight.Id) is NotFoundResult, "guest cannot read another guest receipt");
await tx.RollbackAsync();
Console.WriteLine("All checkout fixtures and payments rolled back; no real gateway called.");

sealed class TestGateway : IOrderPaymentGateway
{
    public bool Fail { get; init; }
    public string Name => "test-only";
    public bool IsConfigured => true;
    public string Authority { get; } = Guid.NewGuid().ToString("N");
    public Task<GatewayStartResult> StartAsync(int id, decimal amount, string callback, string phone, CancellationToken cancellationToken = default) => Task.FromResult(new GatewayStartResult(true, Authority, "https://gateway.test/pay"));
    public Task<GatewayVerifyResult> VerifyAsync(string authority, decimal amount, CancellationToken cancellationToken = default) => Task.FromResult(new GatewayVerifyResult(!Fail && authority == Authority, Fail ? null : "TEST-REFERENCE", DefinitiveFailure: Fail));
}
