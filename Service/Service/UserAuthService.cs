using System.Security.Cryptography;
using Entities;
using Microsoft.EntityFrameworkCore;
using ServiceContract.DTO.DtoCommit;
using ServiceContract.DTO.DtoUser;
using ServiceContract.Interfaces;

namespace Service.Service;

public sealed class UserAuthService : IUserAuthService
{
    private const int SaltSize = 16;
    private const int HashSize = 32;
    private const int Iterations = 100_000;
    private readonly ShopDbContext _db;

    public UserAuthService(ShopDbContext db) => _db = db;

    public async Task<ServiceResponseDto<UserProfileDto>> RegisterAsync(RegisterUserDto model)
    {
        if (model is null) return ServiceResponseDto<UserProfileDto>.Fail("اطلاعات ثبت‌نام ارسال نشده است.");

        var phone = NormalizePhone(model.PhoneNumber);
        if (phone is null) return ServiceResponseDto<UserProfileDto>.Fail("شماره موبایل معتبر نیست.");
        if (string.IsNullOrWhiteSpace(model.FirstName) || string.IsNullOrWhiteSpace(model.LastName)) return ServiceResponseDto<UserProfileDto>.Fail("نام و نام خانوادگی الزامی است.");
        if (string.IsNullOrWhiteSpace(model.Password) || model.Password.Length < 8) return ServiceResponseDto<UserProfileDto>.Fail("رمز عبور باید حداقل ۸ کاراکتر باشد.");

        if (await _db.Users.AnyAsync(x => x.PhoneNumber == phone)) return ServiceResponseDto<UserProfileDto>.Fail("کاربری با این شماره موبایل وجود دارد.");

        var salt = RandomNumberGenerator.GetBytes(SaltSize);
        var user = new CustomerUser
        {
            PhoneNumber = phone,
            Email = string.IsNullOrWhiteSpace(model.Email) ? null : model.Email.Trim().ToLowerInvariant(),
            FirstName = model.FirstName.Trim(),
            LastName = model.LastName.Trim(),
            PasswordSalt = Convert.ToBase64String(salt),
            PasswordHash = HashPassword(model.Password, salt)
        };

        _db.Users.Add(user);
        await _db.SaveChangesAsync();
        return ServiceResponseDto<UserProfileDto>.Success(ToProfile(user));
    }

    public async Task<ServiceResponseDto<UserProfileDto>> LoginAsync(LoginUserDto model)
    {
        if (model is null) return ServiceResponseDto<UserProfileDto>.Fail("اطلاعات ورود ارسال نشده است.");
        var phone = NormalizePhone(model.PhoneNumber);
        if (phone is null || string.IsNullOrEmpty(model.Password)) return ServiceResponseDto<UserProfileDto>.Fail("شماره موبایل یا رمز عبور نادرست است.");

        var user = await _db.Users.SingleOrDefaultAsync(x => x.PhoneNumber == phone);
        if (user is null || !user.IsActive || !VerifyPassword(model.Password, user.PasswordSalt, user.PasswordHash)) return ServiceResponseDto<UserProfileDto>.Fail("شماره موبایل یا رمز عبور نادرست است.");

        user.LastLoginAt = DateTime.UtcNow;
        await _db.SaveChangesAsync();
        return ServiceResponseDto<UserProfileDto>.Success(ToProfile(user));
    }

    public async Task<UserProfileDto?> GetProfileAsync(int userId)
    {
        return await _db.Users.AsNoTracking().Where(x => x.Id == userId && x.IsActive)
            .Select(x => new UserProfileDto { Id = x.Id, PhoneNumber = x.PhoneNumber, Email = x.Email, FirstName = x.FirstName, LastName = x.LastName, BirthDate = x.BirthDate, NationalCode = x.NationalCode, ProfileImageUrl = x.ProfileImageUrl, Role = x.Role, CreatedAt = x.CreatedAt })
            .SingleOrDefaultAsync();
    }

    private static string? NormalizePhone(string? phone)
    {
        if (string.IsNullOrWhiteSpace(phone)) return null;
        var value = new string(phone.Where(char.IsDigit).ToArray());
        if (value.StartsWith("0098")) value = "0" + value[4..];
        else if (value.StartsWith("98") && value.Length == 12) value = "0" + value[2..];
        else if (value.Length == 10 && value.StartsWith('9')) value = "0" + value;
        return value.Length == 11 && value.StartsWith("09") ? value : null;
    }

    private static string HashPassword(string password, byte[] salt) => Convert.ToBase64String(Rfc2898DeriveBytes.Pbkdf2(password, salt, Iterations, HashAlgorithmName.SHA256, HashSize));

    private static bool VerifyPassword(string password, string salt, string hash)
    {
        try { return CryptographicOperations.FixedTimeEquals(Convert.FromBase64String(hash), Convert.FromBase64String(HashPassword(password, Convert.FromBase64String(salt)))); }
        catch (FormatException) { return false; }
    }

    private static UserProfileDto ToProfile(CustomerUser user) => new() { Id = user.Id, PhoneNumber = user.PhoneNumber, Email = user.Email, FirstName = user.FirstName, LastName = user.LastName, BirthDate = user.BirthDate, NationalCode = user.NationalCode, ProfileImageUrl = user.ProfileImageUrl, Role = user.Role, CreatedAt = user.CreatedAt };
}
