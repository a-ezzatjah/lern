using Microsoft.AspNetCore.Mvc;

namespace lern.Controller;

public class CheckoutController : Microsoft.AspNetCore.Mvc.Controller
{
    // Checkout is data-driven (cart/address are loaded per customer cookie),
    // so an old browser/proxy response must never be reused for this page.
    [ResponseCache(Location = ResponseCacheLocation.None, NoStore = true)]
    public IActionResult Index() => View();
}
