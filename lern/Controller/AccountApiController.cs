using System.Security.Claims;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ServiceContract.DTO.DtoUser;
using ServiceContract.Interfaces;

namespace lern.Controller;

[ApiController]
[Route("api/account")]
public sealed class AccountApiController : ControllerBase
{
    private readonly IUserAuthService _users;

    public AccountApiController(IUserAuthService users) => _users = users;

    [HttpPost("register")]
    public async Task<IActionResult> Register([FromBody] RegisterUserDto model)
    {
        var result = await _users.RegisterAsync(model);
        if (!result.Succeeded) return BadRequest(new { success = false, message = result.ErrorMessage ?? result.Errors?.FirstOrDefault() });
        await SignInAsync(result.Data!, true);
        return Created("/api/account/me", new { success = true, user = result.Data });
    }

    [HttpPost("login")]
    public async Task<IActionResult> Login([FromBody] LoginUserDto model)
    {
        var result = await _users.LoginAsync(model);
        if (!result.Succeeded) return Unauthorized(new { success = false, message = result.ErrorMessage ?? result.Errors?.FirstOrDefault() });
        await SignInAsync(result.Data!, model.RememberMe);
        return Ok(new { success = true, user = result.Data });
    }

    [Authorize]
    [HttpPost("logout")]
    public async Task<IActionResult> Logout()
    {
        await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
        return Ok(new { success = true });
    }

    [Authorize]
    [HttpGet("me")]
    public async Task<IActionResult> Me()
    {
        if (!int.TryParse(User.FindFirstValue(ClaimTypes.NameIdentifier), out var userId)) return Unauthorized();
        var user = await _users.GetProfileAsync(userId);
        return user is null ? Unauthorized() : Ok(user);
    }

    private Task SignInAsync(UserProfileDto user, bool isPersistent)
    {
        var claims = new[]
        {
            new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
            new Claim(ClaimTypes.Name, user.FullName),
            new Claim(ClaimTypes.MobilePhone, user.PhoneNumber),
            new Claim(ClaimTypes.Role, user.Role)
        };
        var principal = new ClaimsPrincipal(new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme));
        var customerKeyOptions = new CookieOptions { HttpOnly = true, IsEssential = true, SameSite = SameSiteMode.Lax };
        if (isPersistent) customerKeyOptions.Expires = DateTimeOffset.UtcNow.AddDays(14);
        Response.Cookies.Append("customer-key", $"user-{user.Id}", customerKeyOptions);
        return HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, principal,
            new AuthenticationProperties { IsPersistent = isPersistent, AllowRefresh = isPersistent, ExpiresUtc = isPersistent ? DateTimeOffset.UtcNow.AddDays(14) : null });
    }
}
