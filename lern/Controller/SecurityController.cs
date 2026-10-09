using Microsoft.AspNetCore.Antiforgery;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace lern.Controller;

[ApiController]
public sealed class SecurityController(IAntiforgery antiforgery) : ControllerBase
{
    // For API clients that cannot send a browser Origin: retain the cookie and send
    // the returned request token in X-CSRF-TOKEN. Fetch again after signing in.
    [AllowAnonymous]
    [HttpGet("/api/security/csrf")]
    [ResponseCache(Location = ResponseCacheLocation.None, NoStore = true)]
    public IActionResult Csrf() => Ok(new { token = antiforgery.GetAndStoreTokens(HttpContext).RequestToken });
}
