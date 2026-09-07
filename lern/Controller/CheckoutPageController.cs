using Microsoft.AspNetCore.Mvc;

namespace lern.Controller;

public class CheckoutController : Microsoft.AspNetCore.Mvc.Controller
{
    public IActionResult Index() => View();
}
