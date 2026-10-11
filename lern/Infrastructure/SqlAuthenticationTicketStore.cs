using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using Entities;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.EntityFrameworkCore;

namespace lern.Infrastructure;

public sealed class SqlAuthenticationTicketStore(IServiceScopeFactory scopes, IDataProtectionProvider protection, TimeProvider clock) : ITicketStore
{
    public const string VersionClaim = "lern.authentication-version";
    public static readonly TimeSpan IdleTimeout = TimeSpan.FromMinutes(30);
    public static readonly TimeSpan AbsoluteTimeout = TimeSpan.FromHours(12);
    private readonly IDataProtector protector = protection.CreateProtector("lern.authentication-ticket.v1");

    public async Task<string> StoreAsync(AuthenticationTicket ticket)
    {
        if (!int.TryParse(ticket.Principal.FindFirstValue(ClaimTypes.NameIdentifier), out var userId) ||
            !Guid.TryParse(ticket.Principal.FindFirstValue(VersionClaim), out var version))
            throw new InvalidOperationException("Authentication ticket has no account version.");
        await using var scope = scopes.CreateAsyncScope();
        var db = scope.ServiceProvider.GetRequiredService<ShopDbContext>();
        var role = ticket.Principal.FindFirstValue(ClaimTypes.Role) ?? "";
        if (!await db.Users.AnyAsync(x => x.Id == userId && x.IsActive && x.AuthenticationVersion == version && x.Role == role))
            throw new InvalidOperationException("The account changed before the session could be issued.");
        var now = clock.GetUtcNow().UtcDateTime;
        var maximum = now.Add(AbsoluteTimeout);
        var expiry = ticket.Properties.ExpiresUtc?.UtcDateTime ?? maximum;
        if (expiry > maximum) expiry = maximum;
        ticket.Properties.ExpiresUtc = new DateTimeOffset(expiry, TimeSpan.Zero);
        var key = Convert.ToHexString(RandomNumberGenerator.GetBytes(32));
        db.AuthenticationSessions.Add(new AuthenticationSession
        {
            KeyHash = KeyHash(key), UserId = userId, AuthenticationVersion = version, Role = role,
            ProtectedTicket = protector.Protect(TicketSerializer.Default.Serialize(ticket)),
            CreatedAt = now, LastSeenAt = now, ExpiresAt = expiry
        });
        await db.SaveChangesAsync();
        return key;
    }

    public async Task<AuthenticationTicket?> RetrieveAsync(string key)
    {
        if (!ValidKey(key)) return null;
        await using var scope = scopes.CreateAsyncScope();
        var db = scope.ServiceProvider.GetRequiredService<ShopDbContext>();
        var now = clock.GetUtcNow().UtcDateTime;
        var hash = KeyHash(key);
        var valid = ValidSessions(db, hash, now);
        var data = await valid.AsNoTracking().Select(x => x.ProtectedTicket).SingleOrDefaultAsync();
        if (data is null) return null;
        // Updating an existing valid row cannot resurrect a concurrently deleted/revoked session.
        if (await valid.ExecuteUpdateAsync(setters => setters.SetProperty(x => x.LastSeenAt, x => x.LastSeenAt > now ? x.LastSeenAt : now)) != 1)
            return null;
        try { return TicketSerializer.Default.Deserialize(protector.Unprotect(data)); }
        catch (CryptographicException) { await RemoveAsync(key); return null; }
    }

    public async Task RenewAsync(string key, AuthenticationTicket ticket)
    {
        if (!ValidKey(key)) return;
        await using var scope = scopes.CreateAsyncScope();
        var db = scope.ServiceProvider.GetRequiredService<ShopDbContext>();
        var now = clock.GetUtcNow().UtcDateTime;
        var hash = KeyHash(key);
        var expiry = await ValidSessions(db, hash, now).Select(x => (DateTime?)x.ExpiresAt).SingleOrDefaultAsync();
        if (expiry is null) return;
        ticket.Properties.ExpiresUtc = new DateTimeOffset(expiry.Value, TimeSpan.Zero);
        var data = protector.Protect(TicketSerializer.Default.Serialize(ticket));
        await ValidSessions(db, hash, now).ExecuteUpdateAsync(setters => setters.SetProperty(x => x.ProtectedTicket, data));
    }

    public async Task RemoveAsync(string key)
    {
        if (!ValidKey(key)) return;
        await using var scope = scopes.CreateAsyncScope();
        var db = scope.ServiceProvider.GetRequiredService<ShopDbContext>();
        var hash = KeyHash(key);
        await db.AuthenticationSessions.Where(x => x.KeyHash == hash).ExecuteDeleteAsync();
    }

    private static IQueryable<AuthenticationSession> ValidSessions(ShopDbContext db, string hash, DateTime now)
    {
        var idleCutoff = now.Subtract(IdleTimeout);
        return db.AuthenticationSessions.Where(x => x.KeyHash == hash && x.ExpiresAt > now && x.LastSeenAt > idleCutoff &&
            x.User.IsActive && x.AuthenticationVersion == x.User.AuthenticationVersion && x.Role == x.User.Role);
    }
    private static bool ValidKey(string? key) => key?.Length == 64 && key.All(Uri.IsHexDigit);
    private static string KeyHash(string key) => Convert.ToHexString(SHA256.HashData(Encoding.UTF8.GetBytes(key)));
}
