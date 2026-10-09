using System.Globalization;
using System.Threading.RateLimiting;
using Microsoft.AspNetCore.Antiforgery;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;

namespace lern.Infrastructure;

// Cookie-authenticated JSON APIs need CSRF protection too, including guest baskets.
public sealed class StoreWriteProtection(IAntiforgery antiforgery) : IAsyncAuthorizationFilter, IOrderedFilter
{
    public int Order => 1000;

    public async Task OnAuthorizationAsync(AuthorizationFilterContext context)
    {
        var request = context.HttpContext.Request;
        if (HttpMethods.IsGet(request.Method) || HttpMethods.IsHead(request.Method) ||
            HttpMethods.IsOptions(request.Method)) return;

        var expectedOrigin = $"{request.Scheme}://{request.Host}";
        // Exact authority comparison rejects null, multiple origins, siblings and suffix matches.
        if (string.Equals(request.Headers.Origin.ToString(), expectedOrigin, StringComparison.OrdinalIgnoreCase)) return;
        try { await antiforgery.ValidateRequestAsync(context.HttpContext); }
        catch (AntiforgeryValidationException)
        {
            context.Result = new ObjectResult(new { message = "درخواست معتبر نیست. صفحه را تازه کنید و دوباره تلاش کنید." })
            { StatusCode = StatusCodes.Status400BadRequest };
        }
    }
}

public static class StoreRequestSecurity
{
    public static IServiceCollection AddStoreRequestSecurity(this IServiceCollection services, bool development)
    {
        services.AddAntiforgery(options =>
        {
            options.HeaderName = "X-CSRF-TOKEN";
            options.Cookie.HttpOnly = true;
            options.Cookie.SameSite = SameSiteMode.Strict;
            options.Cookie.SecurePolicy = development ? CookieSecurePolicy.SameAsRequest : CookieSecurePolicy.Always;
        });
        services.AddScoped<StoreWriteProtection>();
        services.AddHsts(options =>
        {
            options.MaxAge = TimeSpan.FromMinutes(5);
            options.IncludeSubDomains = false;
            options.Preload = false;
        });
        services.AddHttpsRedirection(options => options.RedirectStatusCode = StatusCodes.Status301MovedPermanently);
        return services;
    }

    public static IApplicationBuilder UseStoreResponseSecurity(this IApplicationBuilder app) => app.Use(async (context, next) =>
    {
        context.Response.OnStarting(() =>
        {
            var headers = context.Response.Headers;
            headers["X-Content-Type-Options"] = "nosniff";
            headers["X-Frame-Options"] = "DENY";
            headers["Referrer-Policy"] = "strict-origin-when-cross-origin";
            headers["Permissions-Policy"] = "camera=(), microphone=(), geolocation=()";
            // Existing views use inline scripts/styles. Restrict framing and plugins now;
            // a nonce-based script policy needs a separate view/template migration.
            headers["Content-Security-Policy"] = "frame-ancestors 'none'; object-src 'none'; base-uri 'self'";
            if (context.User.Identity?.IsAuthenticated == true ||
                context.Request.Path.StartsWithSegments("/api") ||
                context.Request.Path.StartsWithSegments("/account") ||
                context.Request.Path.StartsWithSegments("/Admin") ||
                context.Request.Path.StartsWithSegments("/checkout"))
            {
                headers.CacheControl = "no-store";
                headers.Pragma = "no-cache";
            }
            return Task.CompletedTask;
        });
        await next();
    });

    public static async ValueTask RejectRateLimit(Microsoft.AspNetCore.RateLimiting.OnRejectedContext context, CancellationToken cancellationToken)
    {
        var retry = context.Lease.TryGetMetadata(MetadataName.RetryAfter, out var duration) ? duration : TimeSpan.FromMinutes(1);
        context.HttpContext.Response.StatusCode = StatusCodes.Status429TooManyRequests;
        context.HttpContext.Response.Headers.RetryAfter = Math.Max(1, (int)Math.Ceiling(retry.TotalSeconds)).ToString(CultureInfo.InvariantCulture);
        context.HttpContext.Response.ContentType = "text/plain; charset=utf-8";
        await context.HttpContext.Response.WriteAsync("تعداد درخواست‌ها بیش از حد مجاز است. لطفاً کمی بعد دوباره تلاش کنید.", cancellationToken);
    }

    public static PartitionedRateLimiter<HttpContext> CreateAuthenticationLimiter() =>
        PartitionedRateLimiter.Create<HttpContext, string>(context =>
        {
            var path = context.Request.Path;
            if (HttpMethods.IsPost(context.Request.Method) &&
                (path.Equals("/api/account/login") || path.Equals("/api/account/register") || path.Equals("/account/change-password")))
                return RateLimitPartition.GetFixedWindowLimiter(context.Connection.RemoteIpAddress?.ToString() ?? "unknown",
                    _ => new FixedWindowRateLimiterOptions
                    { PermitLimit = 10, Window = TimeSpan.FromMinutes(1), QueueLimit = 0, AutoReplenishment = true });
            return RateLimitPartition.GetNoLimiter("other");
        });
}
