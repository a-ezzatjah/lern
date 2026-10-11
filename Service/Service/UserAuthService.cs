using System.Data;
using Entities;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;
using Microsoft.Extensions.Logging;
using Service.Security;
using ServiceContract.DTO.DtoCommit;
using ServiceContract.DTO.DtoUser;
using ServiceContract.Interfaces;

namespace Service.Service;

public sealed class UserAuthService(ShopDbContext db, TimeProvider? time = null, ILogger<UserAuthService>? logger = null) : IUserAuthService
{
    private const string LoginError = "شماره موبایل یا رمز عبور نادرست است.";
    private readonly TimeProvider clock = time ?? TimeProvider.System;

    public async Task<ServiceResponseDto<UserProfileDto>> RegisterAsync(RegisterUserDto model)
    {
        if (model is null) return ServiceResponseDto<UserProfileDto>.Fail("اطلاعات ثبت‌نام ارسال نشده است.");
        var phone = NormalizePhone(model.PhoneNumber);
        if (phone is null) return ServiceResponseDto<UserProfileDto>.Fail("شماره موبایل معتبر نیست.");
        if (string.IsNullOrWhiteSpace(model.FirstName) || string.IsNullOrWhiteSpace(model.LastName) || model.FirstName.Trim().Length > 100 || model.LastName.Trim().Length > 100)
            return ServiceResponseDto<UserProfileDto>.Fail("نام و نام خانوادگی الزامی و حداکثر ۱۰۰ کاراکتر هستند.");
        if (StorePasswords.PolicyError(model.Password) is string error) return ServiceResponseDto<UserProfileDto>.Fail(error);
        var email = string.IsNullOrWhiteSpace(model.Email) ? null : model.Email.Trim().ToLowerInvariant();
        if (email is not null && (email.Length > 256 || !System.Net.Mail.MailAddress.TryCreate(email, out _)))
            return ServiceResponseDto<UserProfileDto>.Fail("ایمیل معتبر نیست.");
        if (await db.Users.AnyAsync(x => x.PhoneNumber == phone || (email != null && x.Email == email)))
            return ServiceResponseDto<UserProfileDto>.Fail("ثبت‌نام با این اطلاعات انجام نشد؛ اطلاعات را بررسی کنید یا وارد حساب شوید.");
        var user = new CustomerUser { PhoneNumber = phone, Email = email, FirstName = model.FirstName.Trim(), LastName = model.LastName.Trim(), CreatedAt = clock.GetUtcNow().UtcDateTime };
        StorePasswords.Set(user, model.Password);
        db.Users.Add(user);
        try { await db.SaveChangesAsync(); }
        catch (DbUpdateException)
        {
            db.Entry(user).State = EntityState.Detached;
            return ServiceResponseDto<UserProfileDto>.Fail("ثبت‌نام انجام نشد؛ لطفاً دوباره تلاش کنید.");
        }
        return ServiceResponseDto<UserProfileDto>.Success(ToProfile(user));
    }

    public async Task<ServiceResponseDto<UserProfileDto>> LoginAsync(LoginUserDto model)
    {
        var phone = NormalizePhone(model?.PhoneNumber);
        var password = model?.Password ?? "";
        if (phone is null || string.IsNullOrEmpty(password))
        {
            StorePasswords.VerifyDummy(password);
            return ServiceResponseDto<UserProfileDto>.Fail(LoginError);
        }
        await using var transaction = await BeginAuthenticationTransaction();
        var user = await UserByPhone(phone).SingleOrDefaultAsync();
        var now = clock.GetUtcNow().UtcDateTime;
        if (user is null || !user.IsActive || user.LockoutUntil > now)
        {
            StorePasswords.VerifyDummy(password);
            logger?.LogWarning("Authentication rejected; account={AccountId}", user?.Id);
            return ServiceResponseDto<UserProfileDto>.Fail(LoginError);
        }
        var verification = StorePasswords.Verify(user, password);
        if (verification == PasswordVerificationResult.Failed)
        {
            if (user.PasswordSalt != StorePasswords.CurrentFormat) StorePasswords.VerifyDummy(password);
            if (user.FailureWindowStartedAt is null || now - user.FailureWindowStartedAt.Value >= TimeSpan.FromMinutes(15))
            { user.FailedLoginCount = 0; user.FailureWindowStartedAt = now; }
            user.FailedLoginCount++;
            if (user.FailedLoginCount >= 5)
            {
                var delay = Math.Min(1440, 15 * (1 << Math.Min(user.LockoutLevel, 7)));
                user.LockoutUntil = now.AddMinutes(delay);
                user.LockoutLevel = Math.Min(user.LockoutLevel + 1, 8);
                user.FailedLoginCount = 0;
                user.FailureWindowStartedAt = null;
                logger?.LogWarning("Authentication account locked; account={AccountId}", user.Id);
            }
            await db.SaveChangesAsync();
            if (transaction is not null) await transaction.CommitAsync();
            logger?.LogWarning("Authentication failed; account={AccountId}", user.Id);
            return ServiceResponseDto<UserProfileDto>.Fail(LoginError);
        }
        if (verification == PasswordVerificationResult.SuccessRehashNeeded) StorePasswords.Set(user, password);
        ResetFailures(user);
        user.LastLoginAt = now;
        try { await db.SaveChangesAsync(); }
        catch (DbUpdateConcurrencyException) { return ServiceResponseDto<UserProfileDto>.Fail(LoginError); }
        if (transaction is not null) await transaction.CommitAsync();
        return ServiceResponseDto<UserProfileDto>.Success(ToProfile(user));
    }

    public async Task<UserProfileDto?> GetProfileAsync(int userId)
    {
        var user = await db.Users.AsNoTracking().SingleOrDefaultAsync(x => x.Id == userId && x.IsActive);
        return user is null ? null : ToProfile(user);
    }

    public async Task<ServiceResponseDto<bool>> ChangePasswordAsync(int userId, string currentPassword, string newPassword)
    {
        await using var transaction = await BeginAuthenticationTransaction();
        var user = await UserById(userId).SingleOrDefaultAsync();
        if (user is null || !user.IsActive || StorePasswords.Verify(user, currentPassword) == PasswordVerificationResult.Failed)
            return ServiceResponseDto<bool>.Fail("رمز عبور فعلی نادرست است.");
        if (StorePasswords.PolicyError(newPassword) is string error) return ServiceResponseDto<bool>.Fail(error);
        if (StorePasswords.Verify(user, newPassword) != PasswordVerificationResult.Failed)
            return ServiceResponseDto<bool>.Fail("رمز عبور جدید باید با رمز عبور فعلی متفاوت باشد.");
        StorePasswords.Set(user, newPassword);
        ResetFailures(user);
        await db.SaveChangesAsync();
        await db.AuthenticationSessions.Where(x => x.UserId == userId).ExecuteDeleteAsync();
        if (transaction is not null) await transaction.CommitAsync();
        logger?.LogInformation("Password changed and sessions revoked; account={AccountId}", userId);
        return ServiceResponseDto<bool>.Success(true);
    }

    public async Task RevokeSessionsAsync(int userId)
    {
        await using var transaction = await BeginAuthenticationTransaction();
        var user = await UserById(userId).SingleOrDefaultAsync();
        if (user is null) return;
        user.AuthenticationVersion = Guid.NewGuid();
        await db.SaveChangesAsync();
        await db.AuthenticationSessions.Where(x => x.UserId == userId).ExecuteDeleteAsync();
        if (transaction is not null) await transaction.CommitAsync();
    }

    private Task<IDbContextTransaction?> BeginAuthenticationTransaction() => db.Database.CurrentTransaction is not null
        ? Task.FromResult<IDbContextTransaction?>(null) : Begin();
    private async Task<IDbContextTransaction?> Begin() => await db.Database.BeginTransactionAsync(
        db.Database.IsSqlServer() ? IsolationLevel.ReadCommitted : IsolationLevel.Serializable);
    private IQueryable<CustomerUser> UserByPhone(string phone) => db.Database.IsSqlServer()
        ? db.Users.FromSqlInterpolated($"SELECT * FROM [Users] WITH (UPDLOCK, ROWLOCK) WHERE [PhoneNumber] = {phone}")
        : db.Users.Where(x => x.PhoneNumber == phone);
    private IQueryable<CustomerUser> UserById(int id) => db.Database.IsSqlServer()
        ? db.Users.FromSqlInterpolated($"SELECT * FROM [Users] WITH (UPDLOCK, ROWLOCK) WHERE [Id] = {id}")
        : db.Users.Where(x => x.Id == id);
    private static void ResetFailures(CustomerUser user)
    { user.FailedLoginCount = 0; user.FailureWindowStartedAt = null; user.LockoutUntil = null; user.LockoutLevel = 0; }

    private static string? NormalizePhone(string? input)
    {
        if (string.IsNullOrWhiteSpace(input)) return null;
        var phone = new string(input.Trim().Select(c => c switch
        { >= '۰' and <= '۹' => (char)('0' + c - '۰'), >= '٠' and <= '٩' => (char)('0' + c - '٠'), _ => c }).ToArray());
        if (phone.StartsWith("+98")) phone = "0" + phone[3..];
        else if (phone.StartsWith("0098")) phone = "0" + phone[4..];
        else if (phone.Length == 12 && phone.StartsWith("98")) phone = "0" + phone[2..];
        else if (phone.Length == 10 && phone.StartsWith('9')) phone = "0" + phone;
        return phone.Length == 11 && phone.StartsWith("09") && phone.All(char.IsAsciiDigit) ? phone : null;
    }
    private static UserProfileDto ToProfile(CustomerUser user) => new()
    {
        Id = user.Id, PhoneNumber = user.PhoneNumber, Email = user.Email, FirstName = user.FirstName, LastName = user.LastName,
        BirthDate = user.BirthDate, NationalCode = user.NationalCode, ProfileImageUrl = user.ProfileImageUrl,
        Role = user.Role, CreatedAt = user.CreatedAt, AuthenticationVersion = user.AuthenticationVersion
    };
}
