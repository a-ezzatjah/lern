using Microsoft.AspNetCore.Mvc;

namespace lern.Controller;

[Route("cart")]
public class CartPageController : Microsoft.AspNetCore.Mvc.Controller
{
    [HttpGet]
    public IActionResult Index() => View();
}
