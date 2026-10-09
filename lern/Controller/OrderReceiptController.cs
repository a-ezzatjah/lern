using Entities;
using lern.Infrastructure;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ServiceContract.Interfaces;
namespace lern.Controller;

[ResponseCache(Location = ResponseCacheLocation.None, NoStore = true)]
public sealed class OrderReceiptController(ShopDbContext db, CustomerSession session, OrderReservations reservations, IOrderPaymentGateway gateway) : Microsoft.AspNetCore.Mvc.Controller
{
    [HttpGet("/orders/{id:int}")]
    public async Task<IActionResult> Details(int id)
    {
        var key = session.GetOrCreate(HttpContext);
        await reservations.ExpireAsync(key);
        var order = await db.Orders.AsNoTracking().Include(x => x.Items).SingleOrDefaultAsync(x => x.Id == id && x.CustomerKey == key);
        if (order is null) return NotFound();
        ViewBag.PaymentAvailable = gateway.IsConfigured;
        return View("~/Views/Checkout/Order.cshtml", order);
    }
}
