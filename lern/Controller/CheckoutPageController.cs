using Microsoft.AspNetCore.Mvc;

namespace lern.Controller;

[Route("checkout")]
public class CheckoutPageController : Microsoft.AspNetCore.Mvc.Controller
{
    [HttpGet]
    public IActionResult Index() => View();
}
