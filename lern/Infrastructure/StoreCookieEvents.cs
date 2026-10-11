using System.Security.Claims;
using Entities;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.EntityFrameworkCore;

namespace lern.Infrastructure;

public sealed class StoreCookieEvents(ShopDbContext db) : CookieAuthenticationEvents
{
    public override async Task ValidatePrincipal(CookieValidatePrincipalContext context)
    {
        if (!int.TryParse(context.Principal?.FindFirstValue(ClaimTypes.NameIdentifier), out var id))
        {
            context.RejectPrincipal();
            await context.HttpContext.SignOutAsync();
            return;
        }
        var user = await db.Users.AsNoTracking().Where(x => x.Id == id && x.IsActive)
            .Select(x => new { x.Role, x.AuthenticationVersion }).SingleOrDefaultAsync();
        if (user is null || !Guid.TryParse(context.Principal?.FindFirstValue(SqlAuthenticationTicketStore.VersionClaim), out var version) ||
            user.AuthenticationVersion != version || context.Principal!.FindFirstValue(ClaimTypes.Role) != user.Role)
        {
            context.RejectPrincipal();
            await context.HttpContext.SignOutAsync();
            return;
        }
    }

    public override Task RedirectToLogin(RedirectContext<CookieAuthenticationOptions> context)
    {
        if (context.Request.Path.StartsWithSegments("/api")) context.Response.StatusCode = StatusCodes.Status401Unauthorized;
        else context.Response.Redirect(context.RedirectUri);
        return Task.CompletedTask;
    }

    public override Task RedirectToAccessDenied(RedirectContext<CookieAuthenticationOptions> context)
    {
        if (context.Request.Path.StartsWithSegments("/api")) context.Response.StatusCode = StatusCodes.Status403Forbidden;
        else context.Response.Redirect(context.RedirectUri);
        return Task.CompletedTask;
    }
}
