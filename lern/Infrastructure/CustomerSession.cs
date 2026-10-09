using Microsoft.AspNetCore.DataProtection;
namespace lern.Infrastructure;

public sealed class CustomerSession(IDataProtectionProvider protection)
{
    private readonly IDataProtector protector = protection.CreateProtector("lern.guest-session.v1");
    public string GetOrCreate(HttpContext context)
    {
        var account = PurchaseLimitPolicy.AccountKey(context.User);
        if (account is not null) return account;
        if (context.Items["GuestCustomerKey"] is string existing) return existing;
        string? key = null;
        var cookie = context.Request.Cookies["guest-session"];
        if (!string.IsNullOrEmpty(cookie))
        {
            try { var value = protector.Unprotect(cookie); if (Guid.TryParseExact(value, "N", out _)) key = value; }
            catch (System.Security.Cryptography.CryptographicException) { }
        }
        // Retain existing anonymous baskets, but never accept a user-* cookie as authentication.
        if (key is null && Guid.TryParseExact(context.Request.Cookies["customer-key"], "N", out var legacy)) key = legacy.ToString("N");
        key ??= Guid.NewGuid().ToString("N");
        context.Items["GuestCustomerKey"] = key;
        if (string.IsNullOrEmpty(cookie) || key != TryUnprotect(cookie))
            context.Response.Cookies.Append("guest-session", protector.Protect(key), new CookieOptions
            { HttpOnly = true, IsEssential = true, SameSite = SameSiteMode.Lax, Secure = context.Request.IsHttps, Path = "/", Expires = DateTimeOffset.UtcNow.AddYears(1) });
        return key;
    }
    private string? TryUnprotect(string value) { try { return protector.Unprotect(value); } catch (System.Security.Cryptography.CryptographicException) { return null; } }
}
