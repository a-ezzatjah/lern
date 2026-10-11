using Entities;
using lern.Infrastructure;
using Microsoft.AspNetCore.Identity;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Service.Security;
using Service.Service;
using ServiceContract.Interfaces;

await using var connection = new SqliteConnection("Data Source=:memory:");
await connection.OpenAsync();
await using var db = new ShopDbContext(new DbContextOptionsBuilder<ShopDbContext>().UseSqlite(connection).Options);
await db.Database.EnsureCreatedAsync();
var clock = new TestClock(); var sender = new TestSender(); var flow = new PhoneAuthentication(db, sender, clock);
var count = 0;
void Check(bool condition, string text) { if (!condition) throw new Exception(text); count++; Console.WriteLine("PASS " + text); }
var phone = "09120000000";
var user = new CustomerUser { PhoneNumber = phone, FirstName = "Test", LastName = "Fixture" };
StorePasswords.Set(user, "Old fixture password 2026"); db.Users.Add(user); await db.SaveChangesAsync();
var disabled = new PhoneAuthentication(db, new UnavailablePhoneCodeSender(), clock);
Check(!(await disabled.RequestAsync(phone, "login")).Succeeded, "unconfigured sender cannot issue a challenge");
Check(!(await disabled.CompleteAsync(phone, Guid.NewGuid(), "123456", "login")).Succeeded, "unconfigured sender cannot authenticate");
Check(!await db.PhoneChallenges.AnyAsync(), "disabled flow does not write to database");
Check(PhoneAuthentication.NormalizePhone("+۹۸۹۱۲۳۴۵۶۷۸۹") == "09123456789", "Persian digits and country prefix normalize");
Check(!(await flow.RequestAsync("bad", "login")).Succeeded, "invalid phone rejected");
Check(!(await flow.RequestAsync(phone, "arbitrary")).Succeeded, "invalid purpose rejected");
var first = await flow.RequestAsync(phone, "login"); var firstCode = sender.Code;
Check(first.Succeeded && first.ChallengeId.HasValue && sender.Sends == 1, "configured sender receives generated code");
var row = await db.PhoneChallenges.SingleAsync();
Check(row.CodeHash != firstCode && !row.CodeHash.Contains(firstCode!) && row.ExpiresAt == clock.GetUtcNow().UtcDateTime.AddMinutes(5), "code stored hashed with five minute expiry");
Check(!(await flow.RequestAsync(phone, "reset")).Succeeded && sender.Sends == 1, "cooldown shared across login and reset");
Check(!(await flow.CompleteAsync(phone, first.ChallengeId!.Value, firstCode, "reset", "A replacement password 2026")).Succeeded, "login code cannot reset password");
Check(!(await flow.CompleteAsync(phone, Guid.NewGuid(), firstCode, "login")).Succeeded, "wrong challenge ID rejected");
var wrongCode = firstCode == "000000" ? "111111" : "000000";
Check(!(await flow.CompleteAsync(phone, first.ChallengeId.Value, wrongCode, "login")).Succeeded, "incorrect code rejected");
Check((await flow.CompleteAsync(phone, first.ChallengeId.Value, firstCode, "login")).UserId == user.Id, "correct OTP authenticates account");
Check(user.PhoneVerifiedAt.HasValue, "successful verification records phone ownership");
Check(!(await flow.CompleteAsync(phone, first.ChallengeId.Value, firstCode, "login")).Succeeded, "consumed OTP cannot be replayed");
clock.Advance(TimeSpan.FromMinutes(1));
var second = await flow.RequestAsync(phone, "login"); var secondCode = sender.Code;
clock.Advance(TimeSpan.FromMinutes(5));
Check(!(await flow.CompleteAsync(phone, second.ChallengeId!.Value, secondCode, "login")).Succeeded, "expired code rejected at boundary");
var third = await flow.RequestAsync(phone, "login"); var thirdCode = sender.Code;
for (var i = 0; i < 5; i++) await flow.CompleteAsync(phone, third.ChallengeId!.Value, thirdCode == "000000" ? "111111" : "000000", "login");
Check(!(await flow.CompleteAsync(phone, third.ChallengeId!.Value, thirdCode, "login")).Succeeded, "five wrong attempts invalidate correct code too");
clock.Advance(TimeSpan.FromMinutes(1));
var fourth = await flow.RequestAsync(phone, "reset"); var fourthCode = sender.Code;
Check(!(await flow.CompleteAsync(phone, fourth.ChallengeId!.Value, fourthCode, "reset", "short")).Succeeded, "password policy enforced before reset");
db.AuthenticationSessions.Add(new AuthenticationSession { KeyHash = new string('a',64), UserId = user.Id, AuthenticationVersion = user.AuthenticationVersion, Role = user.Role, ProtectedTicket = new byte[] { 1 }, CreatedAt = clock.GetUtcNow().UtcDateTime, LastSeenAt = clock.GetUtcNow().UtcDateTime, ExpiresAt = clock.GetUtcNow().UtcDateTime.AddHours(1) });
await db.SaveChangesAsync();
var oldVersion = user.AuthenticationVersion;
Check((await flow.CompleteAsync(phone, fourth.ChallengeId.Value, fourthCode, "reset", "A replacement password 2026")).Succeeded, "verified reset sets new password");
Check(StorePasswords.Verify(user, "A replacement password 2026") != PasswordVerificationResult.Failed && StorePasswords.Verify(user, "Old fixture password 2026") == PasswordVerificationResult.Failed, "new password works and old password fails");
Check(user.AuthenticationVersion != oldVersion && !await db.AuthenticationSessions.AnyAsync(), "reset invalidates version and removes all sessions");
Check(!(await flow.CompleteAsync(phone, fourth.ChallengeId.Value, fourthCode, "reset", "Another fixture password 2026")).Succeeded, "reset cannot be replayed");
clock.Advance(TimeSpan.FromMinutes(1));
Check((await flow.RequestAsync(phone, "login")).Succeeded, "fifth hourly send allowed");
clock.Advance(TimeSpan.FromMinutes(1));
Check(!(await flow.RequestAsync(phone, "reset")).Succeeded, "sixth hourly send blocked across purposes");
clock.Advance(TimeSpan.FromHours(1));
var afterHour = await flow.RequestAsync(phone, "login"); var afterCode = sender.Code;
Check(afterHour.Succeeded, "send budget resets after hour");
user.AuthenticationVersion = Guid.NewGuid(); await db.SaveChangesAsync();
Check(!(await flow.CompleteAsync(phone, afterHour.ChallengeId!.Value, afterCode, "login")).Succeeded, "account change invalidates outstanding code");
clock.Advance(TimeSpan.FromMinutes(1));
var inactive = await flow.RequestAsync(phone, "login"); var inactiveCode = sender.Code;
user.IsActive = false; await db.SaveChangesAsync();
Check(!(await flow.CompleteAsync(phone, inactive.ChallengeId!.Value, inactiveCode, "login")).Succeeded, "disabled account cannot use previously issued code");
var beforeUnknown = sender.Sends;
var unknown = await flow.RequestAsync("09120000001", "login");
Check(unknown.Succeeded && sender.Sends == beforeUnknown, "unknown account receives generic response without sending SMS");
Check(!(await flow.CompleteAsync("09120000001", unknown.ChallengeId!.Value, "123456", "login")).Succeeded, "unknown phone cannot authenticate");
var other = new CustomerUser { PhoneNumber = "09120000002", FirstName = "Other", LastName = "Fixture", LockoutUntil = clock.GetUtcNow().UtcDateTime.AddMinutes(15) };
StorePasswords.Set(other, "Other fixture password 2026"); db.Users.Add(other); await db.SaveChangesAsync();
var beforeLocked = sender.Sends; var locked = await flow.RequestAsync(other.PhoneNumber, "login");
Check(locked.Succeeded && sender.Sends == beforeLocked, "locked account cannot bypass lockout with SMS login");
sender.Fail = true; other.LockoutUntil = null; await db.SaveChangesAsync(); clock.Advance(TimeSpan.FromMinutes(1));
var failed = await flow.RequestAsync(other.PhoneNumber, "login"); var failedCode = sender.Code;
Check(failed.Succeeded && !(await flow.CompleteAsync(other.PhoneNumber, failed.ChallengeId!.Value, failedCode, "login")).Succeeded, "provider failure cannot authenticate and keeps response generic");
Check(!(await flow.RequestAsync(other.PhoneNumber, "reset")).Succeeded, "provider failure still consumes resend budget");
Console.WriteLine($"Phone authentication: {count} checks passed; only disposable in-memory fixtures used, no real SMS sent.");

sealed class TestClock : TimeProvider
{
    private DateTimeOffset now = new(2026, 10, 11, 12, 0, 0, TimeSpan.Zero);
    public override DateTimeOffset GetUtcNow() => now;
    public void Advance(TimeSpan duration) => now += duration;
}
sealed class TestSender : IPhoneCodeSender
{
    public bool IsConfigured => true;
    public string? Code { get; private set; }
    public int Sends { get; private set; }
    public bool Fail { get; set; }
    public Task<bool> SendAsync(string phoneNumber, string code, string purpose, CancellationToken cancellationToken = default)
    { Code = code; Sends++; return Task.FromResult(!Fail); }
}
