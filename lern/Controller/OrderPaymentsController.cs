using lern.Infrastructure;
using Microsoft.AspNetCore.Mvc;
using ServiceContract.Interfaces;
namespace lern.Controller;

public sealed class OrderPaymentsController(OrderPayments payments, CustomerSession session, IOrderPaymentGateway gateway, IConfiguration configuration) : Microsoft.AspNetCore.Mvc.Controller
{
    [HttpGet("/api/payments/status")]
    public IActionResult Status() => Ok(new { available = gateway.IsConfigured, message = gateway.IsConfigured ? "پرداخت آنلاین" : "پرداخت آنلاین هنوز فعال نشده است؛ ثبت سفارش به معنی پرداخت نیست." });

    [HttpPost("/api/orders/{id:int}/payment")]
    public async Task<IActionResult> Start(int id)
    {
        var callbackBase = configuration["Payments:PublicBaseUrl"];
        if (gateway.IsConfigured && (!Uri.TryCreate(callbackBase, UriKind.Absolute, out var uri) || uri.Scheme != "https"))
            return StatusCode(503, new { message = "تنظیمات آدرس بازگشت پرداخت کامل نیست." });
        try
        {
            var result = await payments.StartAsync(id, session.GetOrCreate(HttpContext), (callbackBase ?? "").TrimEnd('/') + "/payments/callback");
            return result.Succeeded ? Ok(result) : StatusCode(503, result);
        }
        catch (HttpRequestException) { return StatusCode(503, new { message = "ارتباط با درگاه برقرار نشد؛ دوباره تلاش کنید." }); }
    }

    [HttpGet("/payments/callback")]
    [ResponseCache(Location = ResponseCacheLocation.None, NoStore = true)]
    public async Task<IActionResult> Callback(string? authority)
    {
        PaymentFlowResult result;
        try { result = await payments.VerifyAsync(authority ?? ""); }
        catch (HttpRequestException) { result = new(false, "نتیجهٔ پرداخت هنوز قابل تأیید نیست؛ برای پیگیری با فروشگاه تماس بگیرید."); }
        return View("~/Views/Checkout/PaymentResult.cshtml", result);
    }
}
