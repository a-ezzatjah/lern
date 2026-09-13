using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace lern.Controller;

public sealed class AccountController : Microsoft.AspNetCore.Mvc.Controller
{
    [AllowAnonymous]
    [HttpGet("/login")]
    public IActionResult Login() => View();

    [AllowAnonymous]
    [HttpGet("/register")]
    public IActionResult Register() => View();

    [Authorize]
    [HttpGet("/account")]
    public IActionResult Index() => View();
}
