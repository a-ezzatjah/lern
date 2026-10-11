using lern.Infrastructure;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ServiceContract.Interfaces;

namespace lern.Controller;

[ApiController, AllowAnonymous, Route("api/account/phone")]
[ResponseCache(Location = ResponseCacheLocation.None, NoStore = true)]
public sealed class PhoneAuthenticationController(PhoneAuthentication phones, IUserAuthService users, TimeProvider clock) : ControllerBase
{
    [HttpGet("status")]
    public IActionResult Status() => Ok(new { available = phones.IsConfigured, message = phones.IsConfigured ? "کد تأیید به شماره ثبت‌شده ارسال می‌شود." : PhoneAuthentication.DisabledMessage });

    [HttpPost("request")]
    public async Task<IActionResult> RequestCode(PhoneCodeRequest request, CancellationToken cancellationToken)
    {
        var result = await phones.RequestAsync(request.PhoneNumber, request.Purpose, cancellationToken);
        if (result.RetryAfter > 0) Response.Headers.RetryAfter = result.RetryAfter.ToString(System.Globalization.CultureInfo.InvariantCulture);
        return result.Succeeded ? Ok(result) : StatusCode(!phones.IsConfigured ? 503 : result.RetryAfter > 0 ? 429 : 400, result);
    }

    [HttpPost("login")]
    public async Task<IActionResult> Login(PhoneCodeVerification request)
    {
        var result = await phones.CompleteAsync(request.PhoneNumber, request.ChallengeId, request.Code, "login");
        if (!result.Succeeded) return StatusCode(phones.IsConfigured ? 400 : 503, result);
        var user = await users.GetProfileAsync(result.UserId!.Value);
        if (user is null) return BadRequest(new { message = "ورود انجام نشد؛ دوباره تلاش کنید." });
        await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
        await HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, StoreSignIn.Principal(user),
            new AuthenticationProperties { IsPersistent = request.RememberMe, AllowRefresh = false, IssuedUtc = clock.GetUtcNow(), ExpiresUtc = clock.GetUtcNow().Add(SqlAuthenticationTicketStore.AbsoluteTimeout) });
        Response.Cookies.Delete("customer-key");
        return Ok(new { success = true });
    }

    [HttpPost("reset-password")]
    public async Task<IActionResult> ResetPassword(PhonePasswordReset request)
    {
        if (request.NewPassword != request.ConfirmPassword) return BadRequest(new { message = "تکرار رمز عبور یکسان نیست." });
        var result = await phones.CompleteAsync(request.PhoneNumber, request.ChallengeId, request.Code, "reset", request.NewPassword);
        if (!result.Succeeded) return StatusCode(phones.IsConfigured ? 400 : 503, result);
        await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
        Response.Cookies.Delete("customer-key");
        return Ok(new { success = true, result.Message });
    }
}

public sealed record PhoneCodeRequest(string PhoneNumber, string Purpose);
public sealed record PhoneCodeVerification(string PhoneNumber, Guid ChallengeId, string Code, bool RememberMe = false);
public sealed record PhonePasswordReset(string PhoneNumber, Guid ChallengeId, string Code, string NewPassword, string ConfirmPassword);
