using System.Security.Cryptography;
using System.Text;
using Entities;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Options;

namespace Service.Security;

public static class StorePasswords
{
    public const string CurrentFormat = "aspnet-identity-v3";
    public const int MaximumLength = 1024;
    private static readonly PasswordHasher<CustomerUser> Hasher = new(Options.Create(new PasswordHasherOptions
    { CompatibilityMode = PasswordHasherCompatibilityMode.IdentityV3, IterationCount = 600_000 }));
    private static readonly CustomerUser DummyUser = new();
    private static readonly string DummyHash = Hasher.HashPassword(DummyUser, Convert.ToBase64String(RandomNumberGenerator.GetBytes(32)));
    private static readonly HashSet<string> CommonPasswords = new(StringComparer.OrdinalIgnoreCase)
    {
        "123456789012345", "1234567890123456", "12345678901234567890", "passwordpassword",
        "password123456789", "qwertyuiopasdfgh", "qwertyuiopasdfghjkl", "iloveyouiloveyou",
        "letmeinletmeinletmein", "adminadminadmin", "111111111111111", "000000000000000"
    };

    public static string? PolicyError(string? password)
    {
        if (password is null) return "رمز عبور را وارد کنید.";
        var length = password.EnumerateRunes().Count();
        if (length is < 15 or > MaximumLength)
            return "رمز عبور باید بین ۱۵ تا ۱۰۲۴ کاراکتر باشد؛ می‌توانید از یک عبارت طولانی استفاده کنید.";
        if (string.IsNullOrWhiteSpace(password) || CommonPasswords.Contains(password) || password.EnumerateRunes().Distinct().Count() == 1)
            return "این رمز عبور بسیار رایج یا قابل حدس است؛ یک عبارت متفاوت انتخاب کنید.";
        return null;
    }

    public static void Set(CustomerUser user, string password)
    {
        user.PasswordHash = Hasher.HashPassword(user, password);
        user.PasswordSalt = CurrentFormat;
    }

    public static PasswordVerificationResult Verify(CustomerUser user, string password)
    {
        // New passwords have a size policy. Existing passwords are verified in full,
        // including previously accepted long passwords and their upgraded hashes.
        if (string.IsNullOrEmpty(password)) return PasswordVerificationResult.Failed;
        try
        {
            if (user.PasswordSalt == CurrentFormat)
                return Hasher.VerifyHashedPassword(user, user.PasswordHash, password);
            var salt = Convert.FromBase64String(user.PasswordSalt);
            var hash = Convert.FromBase64String(user.PasswordHash);
            if (salt.Length != 16 || hash.Length != 32) return PasswordVerificationResult.Failed;
            var candidate = Rfc2898DeriveBytes.Pbkdf2(password, salt, 100_000, HashAlgorithmName.SHA256, 32);
            return CryptographicOperations.FixedTimeEquals(hash, candidate)
                ? PasswordVerificationResult.SuccessRehashNeeded : PasswordVerificationResult.Failed;
        }
        catch (Exception exception) when (exception is FormatException or ArgumentException or CryptographicException)
        { return PasswordVerificationResult.Failed; }
    }

    public static void VerifyDummy(string password) => Hasher.VerifyHashedPassword(DummyUser, DummyHash,
        password.Length <= MaximumLength * 2 ? password : "invalid");
}
