using System.Reflection;
using System.Security.Claims;
using System.Text.Json;
using Entities;
using lern.Controller;
using lern.Infrastructure;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;

void Check(bool condition, string message) { if (!condition) throw new Exception(message); Console.WriteLine("PASS " + message); }
using var settings = JsonDocument.Parse(await File.ReadAllTextAsync("lern/appsettings.json"));
var connection = settings.RootElement.GetProperty("ConnectionStrings").GetProperty("DefaultConnectionstring").GetString();
await using var db = new ShopDbContext(new DbContextOptionsBuilder<ShopDbContext>().UseSqlServer(connection).Options);
if (args.Contains("--inspect") || args.Contains("--apply"))
{
    var phone = args.SkipWhile(x => x != "--phone").Skip(1).FirstOrDefault() ?? throw new Exception("--phone required");
    var target = await db.Users.SingleOrDefaultAsync(x => x.PhoneNumber == phone);
    Console.WriteLine(target is null ? "Target account not registered." : $"Target account found; active={target.IsActive}; role={target.Role}");
    Console.WriteLine("Existing administrators: " + await db.Users.CountAsync(x => x.Role == "Admin"));
    if (args.Contains("--apply"))
    {
        if (target is null || !target.IsActive) throw new Exception("An active registered account is required.");
        target.Role = "Admin";
        await db.SaveChangesAsync();
        Console.WriteLine("Requested account granted Admin role; other accounts unchanged.");
    }
    return;
}
foreach (var type in typeof(AdminController).Assembly.GetTypes().Where(x => x.Name.StartsWith("Admin") && x.Name.EndsWith("Controller")))
    Check(type.GetCustomAttribute<AuthorizeAttribute>()?.Roles == "Admin", type.Name + " requires Admin");
foreach (var method in new[] { "AddAsync", "UpdateAsync", "DeleteAsync", "GetForUpdateAsync" })
    Check(typeof(ProductController).GetMethod(method)!.GetCustomAttribute<AuthorizeAttribute>()?.Roles == "Admin", method + " requires Admin");
await using var transaction = await db.Database.BeginTransactionAsync(System.Data.IsolationLevel.Serializable);
var targetUser = await db.Users.FirstAsync(x => x.IsActive);
await db.Users.ExecuteUpdateAsync(x => x.SetProperty(u => u.Role, "Customer"));
await db.Entry(targetUser).ReloadAsync();
await InitialAdminSetup.PromoteAsync(db, targetUser.PhoneNumber);
await db.Entry(targetUser).ReloadAsync();
Check(targetUser.Role == "Admin", "first admin uses explicitly configured registered phone");
var count = await db.Users.CountAsync(x => x.Role == "Admin");
await InitialAdminSetup.PromoteAsync(db, targetUser.PhoneNumber);
Check(await db.Users.CountAsync(x => x.Role == "Admin") == count, "restarting setup cannot add administrators");
await db.Users.ExecuteUpdateAsync(x => x.SetProperty(u => u.Role, "Customer"));
await db.Entry(targetUser).ReloadAsync();
try { await InitialAdminSetup.PromoteAsync(db, "invalid"); throw new Exception("invalid accepted"); } catch (InvalidOperationException) { Console.WriteLine("PASS invalid phone rejected"); }
try { await InitialAdminSetup.PromoteAsync(db, "09000000000"); throw new Exception("missing account accepted"); } catch (InvalidOperationException) { Console.WriteLine("PASS missing account rejected"); }
Check(!await db.Users.AnyAsync(x => x.Role == "Admin"), "failed setup never promotes another account");
var principal = new ClaimsPrincipal(new ClaimsIdentity(new[] { new Claim(ClaimTypes.NameIdentifier, targetUser.Id.ToString()), new Claim(ClaimTypes.Role, "Admin") }, CookieAuthenticationDefaults.AuthenticationScheme));
var scheme = new AuthenticationScheme(CookieAuthenticationDefaults.AuthenticationScheme, null, typeof(CookieAuthenticationHandler));
var context = new CookieValidatePrincipalContext(new DefaultHttpContext(), scheme, new CookieAuthenticationOptions(), new AuthenticationTicket(principal, CookieAuthenticationDefaults.AuthenticationScheme));
await new StoreCookieEvents(db).ValidatePrincipal(context);
Check(!context.Principal!.IsInRole("Admin") && context.ShouldRenew, "stale admin cookie loses revoked role");
await transaction.RollbackAsync();
Console.WriteLine("All test role changes rolled back.");
