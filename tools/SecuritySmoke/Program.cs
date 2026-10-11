using System.Net;
using System.Security.Claims;
using lern.Infrastructure;
using Microsoft.AspNetCore.Antiforgery;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Abstractions;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

var checks = 0;
void Check(bool ok, string label)
{
    if (!ok) throw new Exception(label);
    Console.WriteLine("PASS " + label);
    checks++;
}

// A separate HTTP pipeline: no store startup, SQL connection, migrations or real accounts.
var builder = WebApplication.CreateBuilder();
builder.Logging.ClearProviders();
builder.WebHost.UseUrls("http://127.0.0.1:0");
builder.Services.AddStoreRequestSecurity(true);
await using var app = builder.Build();
app.UseStoreResponseSecurity();
app.MapGet("/token", (HttpContext http, IAntiforgery anti) => Results.Text(anti.GetAndStoreTokens(http).RequestToken!));
app.MapMethods("/write", new[] { "POST", "PUT", "DELETE" }, async (HttpContext http, StoreWriteProtection filter) =>
{
    var action = new ActionContext(http, new RouteData(), new ActionDescriptor());
    var context = new AuthorizationFilterContext(action, new List<IFilterMetadata>());
    await filter.OnAuthorizationAsync(context);
    return context.Result is null ? Results.Ok() : Results.StatusCode(400);
});
app.MapGet("/private", (HttpContext http) =>
{
    http.User = new ClaimsPrincipal(new ClaimsIdentity(new[] { new Claim(ClaimTypes.NameIdentifier, "1") }, "test"));
    return Results.Text("synthetic private page");
});
await app.StartAsync();
var origin = app.Urls.Single();
using var client = new HttpClient(new HttpClientHandler { CookieContainer = new CookieContainer() }) { BaseAddress = new Uri(origin) };
foreach (var method in new[] { HttpMethod.Post, HttpMethod.Put, HttpMethod.Delete })
{
    foreach (var untrusted in new[] { "", "null", "https://other.invalid", origin + ".invalid", origin + "/" })
    {
        using var request = new HttpRequestMessage(method, "/write");
        if (untrusted != "") request.Headers.TryAddWithoutValidation("Origin", untrusted);
        using var response = await client.SendAsync(request);
        Check(response.StatusCode == HttpStatusCode.BadRequest, method + " rejects missing/untrusted origin without token");
    }
    using var validRequest = new HttpRequestMessage(method, "/write");
    validRequest.Headers.Add("Origin", origin);
    using var validResponse = await client.SendAsync(validRequest);
    Check(validResponse.IsSuccessStatusCode, method + " accepts exact same origin");
}
var token = await client.GetStringAsync("/token");
using (var request = new HttpRequestMessage(HttpMethod.Post, "/write"))
{
    request.Headers.Add("X-CSRF-TOKEN", token);
    using var response = await client.SendAsync(request);
    Check(response.IsSuccessStatusCode, "non-browser client with cookie and valid antiforgery token works");
}
using (var request = new HttpRequestMessage(HttpMethod.Post, "/write"))
{
    request.Headers.Add("X-CSRF-TOKEN", "invalid-test-token");
    using var response = await client.SendAsync(request);
    Check(response.StatusCode == HttpStatusCode.BadRequest, "invalid antiforgery token rejected");
}
using (var response = await client.GetAsync("/private"))
{
    Check(response.Headers.CacheControl?.NoStore == true, "authenticated response is not cached");
    Check(response.Headers.GetValues("Content-Security-Policy").Single().Contains("frame-ancestors 'none'"), "framing blocked by CSP");
    Check(response.Headers.GetValues("X-Content-Type-Options").Single() == "nosniff", "content sniffing blocked");
}
using (var limiter = StoreRequestSecurity.CreateAuthenticationLimiter())
{
    var http = new DefaultHttpContext();
    http.Request.Method = "POST";
    http.Request.Path = "/api/account/login";
    http.Connection.RemoteIpAddress = IPAddress.Loopback;
    for (var i = 0; i < 10; i++)
    {
        using var lease = await limiter.AcquireAsync(http);
        Check(lease.IsAcquired, "login within IP limit permitted");
    }
    using var rejected = await limiter.AcquireAsync(http);
    Check(!rejected.IsAcquired, "login exceeding IP limit blocked");
    http.Request.Path = "/api/account/register";
    using var registration = await limiter.AcquireAsync(http);
    Check(!registration.IsAcquired, "registration shares authentication IP budget");
    http.Request.Path = "/API/ACCOUNT/LOGIN/";
    using var variant = await limiter.AcquireAsync(http);
    Check(!variant.IsAcquired, "route casing and trailing slash share the same login budget");
    http.Connection.RemoteIpAddress = IPAddress.Parse("127.0.0.2");
    using var independent = await limiter.AcquireAsync(http);
    Check(independent.IsAcquired, "different IP has independent budget");
}
await app.StopAsync();
Console.WriteLine($"Security smoke: {checks} checks passed; no store database accessed.");
