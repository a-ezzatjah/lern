using System.Data;
using System.Globalization;
using System.Security.Cryptography;
using Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;
using Service.Security;
using ServiceContract.Interfaces;

namespace lern.Infrastructure;

public sealed record PhoneFlowResult(bool Succeeded, string Message, Guid? ChallengeId = null, int? UserId = null, int RetryAfter = 0);

public sealed class PhoneAuthentication(ShopDbContext db, IPhoneCodeSender sender, TimeProvider clock)
{
    public const string DisabledMessage = "سرویس پیامک هنوز فعال نیست. فعلاً با رمز عبور وارد شوید.";
    private const string InvalidCode = "کد تأیید معتبر نیست یا مهلت آن تمام شده است.";
    public bool IsConfigured => sender.IsConfigured;

    public static string? NormalizePhone(string? input)
    {
        if (string.IsNullOrWhiteSpace(input) || input.Length > 32) return null;
        var phone = NormalizeDigits(input.Trim());
        if (phone.StartsWith("+98")) phone = "0" + phone[3..];
        else if (phone.StartsWith("0098")) phone = "0" + phone[4..];
        else if (phone.Length == 12 && phone.StartsWith("98")) phone = "0" + phone[2..];
        else if (phone.Length == 10 && phone.StartsWith('9')) phone = "0" + phone;
        return phone.Length == 11 && phone.StartsWith("09") && phone.All(char.IsAsciiDigit) ? phone : null;
    }

    private static string NormalizeDigits(string input) => new(input.Select(c => c switch
    { >= '۰' and <= '۹' => (char)('0' + c - '۰'), >= '٠' and <= '٩' => (char)('0' + c - '٠'), _ => c }).ToArray());

    public async Task<PhoneFlowResult> RequestAsync(string? rawPhone, string purpose, CancellationToken cancellationToken = default)
    {
        if (!sender.IsConfigured) return new(false, DisabledMessage);
        var phone = NormalizePhone(rawPhone);
        if (phone is null || purpose is not ("login" or "reset")) return new(false, "شماره موبایل معتبر نیست.");
        await using var tx = await BeginAsync();
        await LockPhoneAsync(phone, cancellationToken);
        var now = clock.GetUtcNow().UtcDateTime;
        var challenge = await db.PhoneChallenges.SingleOrDefaultAsync(x => x.PhoneNumber == phone, cancellationToken);
        if (challenge is not null)
        {
            var wait = 60 - (int)(now - challenge.LastSentAt).TotalSeconds;
            if (wait > 0) return new(false, "برای ارسال مجدد کد کمی صبر کنید.", RetryAfter: wait);
            if (now - challenge.WindowStartedAt < TimeSpan.FromHours(1) && challenge.SendCount >= 5)
                return new(false, "سقف ارسال کد تکمیل شده است؛ بعداً دوباره تلاش کنید.", RetryAfter: Math.Max(1, (int)(challenge.WindowStartedAt.AddHours(1) - now).TotalSeconds));
        }
        else { challenge = new PhoneChallenge { PhoneNumber = phone }; db.PhoneChallenges.Add(challenge); }
        var user = await db.Users.AsNoTracking().SingleOrDefaultAsync(x => x.PhoneNumber == phone && x.IsActive, cancellationToken);
        if (now - challenge.WindowStartedAt >= TimeSpan.FromHours(1)) { challenge.SendCount = 0; challenge.WindowStartedAt = now; }
        var code = RandomNumberGenerator.GetInt32(0, 1_000_000).ToString("D6", CultureInfo.InvariantCulture);
        challenge.ChallengeId = Guid.NewGuid(); challenge.Purpose = purpose;
        challenge.Salt = Convert.ToBase64String(RandomNumberGenerator.GetBytes(16));
        challenge.CodeHash = Hash(code, challenge.Salt);
        challenge.UserId = user?.Id; challenge.AuthenticationVersion = user?.AuthenticationVersion ?? Guid.Empty;
        challenge.ExpiresAt = now.AddMinutes(5); challenge.LastSentAt = now;
        challenge.SendCount++; challenge.AttemptCount = 0; challenge.Consumed = false;
        // Unknown, inactive and locked accounts receive the same public result; never send their code.
        if (user is not null && (purpose == "reset" || user.LockoutUntil is null || user.LockoutUntil <= now))
        {
            try
            {
                using var deadline = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
                deadline.CancelAfter(TimeSpan.FromSeconds(15));
                if (!await sender.SendAsync(phone, code, purpose, deadline.Token))
                    challenge.Consumed = true;
            }
            catch (Exception error) when (error is HttpRequestException or OperationCanceledException)
            { challenge.Consumed = true; }
        }
        // Keep resend budgets even when delivery fails; do not leak whether a phone has an account.
        if (challenge.Consumed) { challenge.CodeHash = ""; challenge.Salt = ""; }
        await db.SaveChangesAsync(cancellationToken);
        if (tx is not null) await tx.CommitAsync(cancellationToken);
        return new(true, "اگر حساب فعالی با این شماره وجود داشته باشد، کد تأیید ارسال می‌شود.", challenge.ChallengeId, RetryAfter: 60);
    }

    public async Task<PhoneFlowResult> CompleteAsync(string? rawPhone, Guid challengeId, string? rawCode, string purpose, string? newPassword = null)
    {
        if (!sender.IsConfigured) return new(false, DisabledMessage);
        var phone = NormalizePhone(rawPhone);
        if (phone is null || rawCode is null || rawCode.Length != 6 || purpose is not ("login" or "reset")) return new(false, InvalidCode);
        var code = NormalizeDigits(rawCode);
        if (!code.All(char.IsAsciiDigit)) return new(false, InvalidCode);
        if (purpose == "reset" && StorePasswords.PolicyError(newPassword) is string error) return new(false, error);
        await using var tx = await BeginAsync();
        await LockPhoneAsync(phone, CancellationToken.None);
        var challenge = await db.PhoneChallenges.SingleOrDefaultAsync(x => x.PhoneNumber == phone);
        var now = clock.GetUtcNow().UtcDateTime;
        if (challenge is null || challenge.ChallengeId != challengeId || challenge.Purpose != purpose || challenge.Consumed || challenge.ExpiresAt <= now || challenge.AttemptCount >= 5)
            return new(false, InvalidCode);
        challenge.AttemptCount++;
        var matches = CryptographicOperations.FixedTimeEquals(Convert.FromBase64String(Hash(code, challenge.Salt)), Convert.FromBase64String(challenge.CodeHash));
        var user = db.Database.IsSqlServer()
            ? await db.Users.FromSqlInterpolated($"SELECT * FROM [Users] WITH (UPDLOCK, ROWLOCK) WHERE [PhoneNumber] = {phone}").SingleOrDefaultAsync()
            : await db.Users.SingleOrDefaultAsync(x => x.PhoneNumber == phone);
        if (!matches || user is null || !user.IsActive || user.Id != challenge.UserId || user.AuthenticationVersion != challenge.AuthenticationVersion || (purpose == "login" && user.LockoutUntil > now))
        {
            if (challenge.AttemptCount >= 5) { challenge.Consumed = true; challenge.CodeHash = ""; }
            await db.SaveChangesAsync(); if (tx is not null) await tx.CommitAsync();
            return new(false, InvalidCode);
        }
        challenge.Consumed = true; challenge.CodeHash = ""; challenge.Salt = "";
        user.PhoneVerifiedAt = now;
        if (purpose == "reset")
        {
            StorePasswords.Set(user, newPassword!);
            user.AuthenticationVersion = Guid.NewGuid();
            user.FailedLoginCount = 0; user.FailureWindowStartedAt = null; user.LockoutUntil = null; user.LockoutLevel = 0;
            await db.AuthenticationSessions.Where(x => x.UserId == user.Id).ExecuteDeleteAsync();
        }
        else user.LastLoginAt = now;
        await db.SaveChangesAsync(); if (tx is not null) await tx.CommitAsync();
        return new(true, purpose == "reset" ? "رمز عبور تغییر کرد. با رمز جدید وارد شوید؛ نشست‌های قبلی بسته شدند." : "شماره موبایل تأیید شد.", UserId: user.Id);
    }

    private static string Hash(string code, string salt) => Convert.ToBase64String(Rfc2898DeriveBytes.Pbkdf2(code, Convert.FromBase64String(salt), 100_000, HashAlgorithmName.SHA256, 32));
    private async Task<IDbContextTransaction?> BeginAsync() => db.Database.CurrentTransaction is not null ? null : await db.Database.BeginTransactionAsync(IsolationLevel.Serializable);
    private async Task LockPhoneAsync(string phone, CancellationToken cancellationToken)
    {
        if (!db.Database.IsSqlServer()) return;
        var resource = "lern.phone-auth." + phone;
        await db.Database.ExecuteSqlInterpolatedAsync($"DECLARE @result int; EXEC @result = sys.sp_getapplock @Resource={resource}, @LockMode='Exclusive', @LockOwner='Transaction', @LockTimeout=20000; IF @result < 0 THROW 51000, 'Phone authentication lock unavailable', 1;", cancellationToken);
    }
}
