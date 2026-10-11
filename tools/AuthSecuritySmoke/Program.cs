using System.Buffers.Binary;
using System.Net;
using System.Net.Http.Json;
using System.Security.Claims;
using System.Security.Cryptography;
using Entities;
using lern.Controller;
using lern.Infrastructure;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Service.Security;
using Service.Service;
using ServiceContract.DTO.DtoUser;
using ServiceContract.Interfaces;

// This executable never reads the store connection string or executes store startup.
await using var database = new SqliteConnection("Data Source=:memory:");
await database.OpenAsync();
var clock = new TestClock(DateTimeOffset.UtcNow);
var builder = WebApplication.CreateBuilder();
builder.Logging.ClearProviders();
builder.WebHost.UseUrls("http://127.0.0.1:0");
builder.Services.AddSingleton<TimeProvider>(clock);
builder.Services.AddSingleton<IDataProtectionProvider>(new EphemeralDataProtectionProvider());
builder.Services.AddDbContext<ShopDbContext>(options => options.UseSqlite(database));
builder.Services.AddScoped<IUserAuthService, UserAuthService>();
builder.Services.AddScoped<StoreCookieEvents>();
builder.Services.AddSingleton<SqlAuthenticationTicketStore>();
builder.Services.AddSingleton<IPostConfigureOptions<CookieAuthenticationOptions>, StoreCookieConfiguration>();
builder.Services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme).AddCookie(options =>
{
    options.Cookie.Name = "lern.auth.test";
    options.Cookie.HttpOnly = true;
    options.Cookie.SecurePolicy = CookieSecurePolicy.Always;
    options.Cookie.SameSite = SameSiteMode.Lax;
    options.EventsType = typeof(StoreCookieEvents);
});
builder.Services.AddAuthorization();
builder.Services.AddStoreRequestSecurity(true);
builder.Services.AddControllers(options => options.Filters.AddService<StoreWriteProtection>())
    .AddApplicationPart(typeof(AccountApiController).Assembly);
await using var app = builder.Build();
// Emulate TLS termination solely to assert Secure flags. This is not a TLS transport test.
app.Use(async (context, next) => { context.Request.Scheme = "https"; await next(); });
app.UseStoreResponseSecurity();
app.UseRouting();
app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();
var checks = 0;
void Check(bool ok, string label)
{
    if (!ok) throw new Exception(label);
    Console.WriteLine("PASS " + label);
    checks++;
}
async Task<T> InScope<T>(Func<ShopDbContext, IUserAuthService, Task<T>> action)
{
    await using var scope = app.Services.CreateAsyncScope();
    return await action(scope.ServiceProvider.GetRequiredService<ShopDbContext>(), scope.ServiceProvider.GetRequiredService<IUserAuthService>());
}
await InScope(async (db, _) => { await db.Database.EnsureCreatedAsync(); return true; });
const string phone = "09000000001";
const string legacyPassword = "SyntheticOld8!";
const string newPassword = "synthetic updated password phrase";
var legacySalt = RandomNumberGenerator.GetBytes(16);
var userId = await InScope(async (db, _) =>
{
    var user = new CustomerUser
    {
        PhoneNumber = phone, FirstName = "Synthetic", LastName = "Fixture",
        PasswordSalt = Convert.ToBase64String(legacySalt),
        PasswordHash = Convert.ToBase64String(Rfc2898DeriveBytes.Pbkdf2(legacyPassword, legacySalt, 100_000, HashAlgorithmName.SHA256, 32))
    };
    db.Users.Add(user); await db.SaveChangesAsync(); return user.Id;
});
Check(StorePasswords.PolicyError("short") is not null, "short new password rejected");
Check(StorePasswords.PolicyError("passwordpassword") is not null, "local common-password blocklist enforced");
Check(StorePasswords.PolicyError(string.Concat(Enumerable.Repeat("😀", 14))) is not null, "minimum counts Unicode code points");
Check(StorePasswords.PolicyError("یک عبارت طولانی ساختگی برای آزمون") is null, "Unicode and spaces permitted");
Check(StorePasswords.PolicyError(new string('a', 80) + " ending") is null, "passwords longer than 64 characters permitted");
var longLegacyPassword = new string('x', 3000) + "synthetic fixture";
Check(StorePasswords.PolicyError(longLegacyPassword) is not null, "new password size policy enforced");
await InScope(async (db, _) =>
{
    var salt = RandomNumberGenerator.GetBytes(16);
    db.Users.Add(new CustomerUser
    {
        PhoneNumber = "09000000004", FirstName = "Long", LastName = "Fixture",
        PasswordSalt = Convert.ToBase64String(salt),
        PasswordHash = Convert.ToBase64String(Rfc2898DeriveBytes.Pbkdf2(longLegacyPassword, salt, 100_000, HashAlgorithmName.SHA256, 32))
    });
    await db.SaveChangesAsync(); return true;
});
Check((await InScope((_, auth) => auth.LoginAsync(new LoginUserDto { PhoneNumber = "09000000004", Password = longLegacyPassword }))).Succeeded,
    "previously accepted long legacy password is verified fully and upgraded");
Check((await InScope((_, auth) => auth.LoginAsync(new LoginUserDto { PhoneNumber = "09000000004", Password = longLegacyPassword }))).Succeeded,
    "upgraded long password remains valid without truncation");
var registration = await InScope((_, auth) => auth.RegisterAsync(new RegisterUserDto
{ PhoneNumber = "09000000002", FirstName = "Other", LastName = "Fixture", Password = "another synthetic password phrase" }));
Check(registration.Succeeded, "new registrations use stronger format");
var otherId = registration.Data!.Id;
var missing = await InScope((_, auth) => auth.LoginAsync(new LoginUserDto { PhoneNumber = "09000000003", Password = "incorrect fixture password" }));
var wrong = await InScope((_, auth) => auth.LoginAsync(new LoginUserDto { PhoneNumber = phone, Password = "incorrect fixture password" }));
Check(!wrong.Succeeded && missing.ErrorMessage == wrong.ErrorMessage, "missing account and wrong password have identical errors");
for (var attempt = 0; attempt < 4; attempt++)
    await InScope((_, auth) => auth.LoginAsync(new LoginUserDto { PhoneNumber = phone, Password = "incorrect fixture password" }));
var correctWhileLocked = await InScope((_, auth) => auth.LoginAsync(new LoginUserDto { PhoneNumber = phone, Password = legacyPassword }));
Check(!correctWhileLocked.Succeeded, "fifth failure locks account even for correct password");
Check(await InScope(async (db, _) => (await db.Users.FindAsync(userId))!.LockoutUntil == clock.GetUtcNow().UtcDateTime.AddMinutes(15)), "initial lockout lasts 15 minutes");
clock.Advance(TimeSpan.FromMinutes(16));
for (var attempt = 0; attempt < 5; attempt++)
    await InScope((_, auth) => auth.LoginAsync(new LoginUserDto { PhoneNumber = phone, Password = "incorrect fixture password" }));
Check(await InScope(async (db, _) => (await db.Users.FindAsync(userId))!.LockoutUntil == clock.GetUtcNow().UtcDateTime.AddMinutes(30)), "repeated lockout backs off to 30 minutes");
clock.Advance(TimeSpan.FromMinutes(31));
var legacyLogin = await InScope((_, auth) => auth.LoginAsync(new LoginUserDto { PhoneNumber = "+989000000001", Password = legacyPassword }));
Check(legacyLogin.Succeeded, "existing short legacy password still logs in after lockout expires");
Check(await InScope(async (db, _) =>
{
    var user = (await db.Users.FindAsync(userId))!;
    var hash = Convert.FromBase64String(user.PasswordHash);
    return user.PasswordSalt == StorePasswords.CurrentFormat && hash[0] == 1 &&
        BinaryPrimitives.ReadUInt32BigEndian(hash.AsSpan(5, 4)) == 600_000 &&
        BinaryPrimitives.ReadUInt32BigEndian(hash.AsSpan(1, 4)) == 2 && user.FailedLoginCount == 0 && user.LockoutLevel == 0;
}), "legacy hash upgrades to framework PBKDF2-SHA512 600000 rounds and clears lock state");
await app.StartAsync();
var address = app.Urls.Single();
var origin = address.Replace("http://", "https://");
using var client = new HttpClient(new HttpClientHandler { UseCookies = false, AllowAutoRedirect = false }) { BaseAddress = new Uri(address) };
async Task<HttpResponseMessage> Send(HttpMethod method, string route, string? cookie = null, object? body = null, bool trustedOrigin = true)
{
    using var request = new HttpRequestMessage(method, route);
    if (cookie is not null) request.Headers.Add("Cookie", cookie);
    if (trustedOrigin) request.Headers.Add("Origin", origin);
    if (body is not null) request.Content = JsonContent.Create(body);
    return await client.SendAsync(request);
}
async Task<string> Login(string fixturePhone = phone, string password = legacyPassword)
{
    using var response = await Send(HttpMethod.Post, "/api/account/login", body: new { phoneNumber = fixturePhone, password, rememberMe = true });
    Check(response.IsSuccessStatusCode, "HTTP fixture login succeeds");
    var cookie = response.Headers.GetValues("Set-Cookie").Last(x => x.StartsWith("lern.auth.test="));
    Check(cookie.Contains("secure", StringComparison.OrdinalIgnoreCase) && cookie.Contains("httponly", StringComparison.OrdinalIgnoreCase) && cookie.Contains("samesite=lax", StringComparison.OrdinalIgnoreCase), "authentication cookie has Secure HttpOnly SameSite=Lax");
    var json = await response.Content.ReadAsStringAsync();
    Check(!json.Contains("authenticationVersion", StringComparison.OrdinalIgnoreCase), "internal account version is omitted from JSON");
    return cookie.Split(';')[0];
}
async Task<bool> Accessible(string cookie)
{
    using var response = await Send(HttpMethod.Get, "/api/account/me", cookie);
    Check(response.Headers.CacheControl?.NoStore == true, "account API does not cache private data");
    if (response.StatusCode is not (HttpStatusCode.OK or HttpStatusCode.Unauthorized))
        throw new Exception("Expected account data or 401; server errors must not count as session revocation.");
    return response.StatusCode == HttpStatusCode.OK;
}
using (var unknownResponse = await Send(HttpMethod.Post, "/api/account/login", body: new { phoneNumber = "09000000003", password = "incorrect fixture password" }))
using (var wrongResponse = await Send(HttpMethod.Post, "/api/account/login", body: new { phoneNumber = phone, password = "incorrect fixture password" }))
    Check(unknownResponse.StatusCode == HttpStatusCode.Unauthorized && wrongResponse.StatusCode == HttpStatusCode.Unauthorized &&
        await unknownResponse.Content.ReadAsStringAsync() == await wrongResponse.Content.ReadAsStringAsync(),
        "unknown account and wrong password return identical HTTP status and body");
var cookieA = await Login();
var cookieB = await Login();
Check(cookieA != cookieB && await Accessible(cookieA) && await Accessible(cookieB), "each device gets an independent random session");
using (var response = await Send(HttpMethod.Post, "/api/account/logout", cookieA, trustedOrigin: false))
    Check(response.StatusCode == HttpStatusCode.BadRequest, "logout rejects request without CSRF defense");
using (var response = await Send(HttpMethod.Post, "/api/account/logout", cookieA))
    Check(response.IsSuccessStatusCode, "logout succeeds");
Check(!await Accessible(cookieA) && await Accessible(cookieB), "logout revokes only that device on server");
var changed = await InScope((_, auth) => auth.ChangePasswordAsync(userId, legacyPassword, newPassword));
Check(changed.Succeeded && !await Accessible(cookieB), "password change invalidates other devices");
Check(!(await InScope((_, auth) => auth.LoginAsync(new LoginUserDto { PhoneNumber = phone, Password = legacyPassword }))).Succeeded, "old password no longer works after change");
var cookieC = await Login(phone, newPassword);
var otherCookie = await Login("09000000002", "another synthetic password phrase");
using (var response = await Send(HttpMethod.Post, "/api/account/logout-all", cookieC))
    Check(response.IsSuccessStatusCode, "logout-all endpoint succeeds");
Check(!await Accessible(cookieC) && await Accessible(otherCookie), "logout-all revokes this account without affecting another user");
var roleCookie = await Login(phone, newPassword);
await InScope(async (db, _) => { var user = (await db.Users.FindAsync(userId))!; user.Role = "Admin"; await db.SaveChangesAsync(); return true; });
Check(!await Accessible(roleCookie), "role change requires new authentication");
var disableCookie = await Login(phone, newPassword);
await InScope(async (db, _) => { var user = (await db.Users.FindAsync(userId))!; user.IsActive = false; await db.SaveChangesAsync(); return true; });
Check(!await Accessible(disableCookie), "disabled account cannot use old session");
await InScope(async (db, _) => { var user = (await db.Users.FindAsync(userId))!; user.IsActive = true; await db.SaveChangesAsync(); return true; });
Check(!await Accessible(disableCookie), "re-enabling account does not revive old sessions");
var idleCookie = await Login(phone, newPassword);
clock.Advance(TimeSpan.FromMinutes(30));
Check(!await Accessible(idleCookie), "30-minute server idle timeout is enforced");
var absoluteCookie = await Login(phone, newPassword);
for (var interval = 0; interval < 35; interval++)
{
    clock.Advance(TimeSpan.FromMinutes(20));
    using var response = await Send(HttpMethod.Get, "/api/account/me", absoluteCookie);
    if (!response.IsSuccessStatusCode) throw new Exception("active session unexpectedly expired");
}
clock.Advance(TimeSpan.FromMinutes(20));
Check(!await Accessible(absoluteCookie), "activity cannot extend 12-hour absolute timeout");
var store = app.Services.GetRequiredService<SqlAuthenticationTicketStore>();
var profile = await InScope((_, auth) => auth.GetProfileAsync(userId));
var principal = new ClaimsPrincipal(new ClaimsIdentity(new[]
{
    new Claim(ClaimTypes.NameIdentifier, userId.ToString()), new Claim(ClaimTypes.Role, profile!.Role),
    new Claim(SqlAuthenticationTicketStore.VersionClaim, profile.AuthenticationVersion.ToString())
}, CookieAuthenticationDefaults.AuthenticationScheme));
var ticket = new AuthenticationTicket(principal, new AuthenticationProperties(), CookieAuthenticationDefaults.AuthenticationScheme);
var key = await store.StoreAsync(ticket);
await store.RemoveAsync(key);
await store.RenewAsync(key, ticket);
Check(await store.RetrieveAsync(key) is null, "renewal cannot recreate a revoked session");
Check(await store.RetrieveAsync("invalid") is null, "malformed session key is rejected");
Check(await InScope(async (db, _) =>
{
    await AuthenticationSessionWorker.CleanupAsync(db, clock);
    return !await db.AuthenticationSessions.AnyAsync();
}), "cleanup removes expired and invalid sessions from storage");
await InScope(async (db, _) => { db.Users.Remove((await db.Users.FindAsync(otherId))!); await db.SaveChangesAsync(); return true; });
Check(!await Accessible(otherCookie), "deleted account sessions cannot authenticate");
await app.StopAsync();
Console.WriteLine($"Auth security: {checks} checks passed using only disposable in-memory fixtures; real store untouched.");

sealed class TestClock(DateTimeOffset initial) : TimeProvider
{
    private DateTimeOffset current = initial;
    public override DateTimeOffset GetUtcNow() => current;
    public void Advance(TimeSpan duration) => current = current.Add(duration);
}
