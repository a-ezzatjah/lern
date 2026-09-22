using System.Security.Claims;
using Entities;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ServiceContract.DTO.DtoUser;
using ServiceContract.Interfaces;

namespace lern.Controller;

[ApiController]
[Route("api/account")]
public sealed class AccountApiController : ControllerBase
{
    private readonly IUserAuthService _users;
    private readonly ShopDbContext _db;
    private readonly IWebHostEnvironment _environment;

    public AccountApiController(IUserAuthService users, ShopDbContext db, IWebHostEnvironment environment)
    {
        _users = users;
        _db = db;
        _environment = environment;
    }

    [HttpPost("register")]
    public async Task<IActionResult> Register([FromBody] RegisterUserDto model)
    {
        var result = await _users.RegisterAsync(model);
        if (!result.Succeeded) return BadRequest(new { success = false, message = result.ErrorMessage ?? result.Errors?.FirstOrDefault() });
        await SignInAsync(result.Data!, true);
        return Created("/api/account/me", new { success = true, user = result.Data });
    }

    [HttpPost("login")]
    public async Task<IActionResult> Login([FromBody] LoginUserDto model)
    {
        var result = await _users.LoginAsync(model);
        if (!result.Succeeded) return Unauthorized(new { success = false, message = result.ErrorMessage ?? result.Errors?.FirstOrDefault() });
        await SignInAsync(result.Data!, model.RememberMe);
        return Ok(new { success = true, user = result.Data });
    }

    [Authorize]
    [HttpPost("logout")]
    public async Task<IActionResult> Logout()
    {
        await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
        return Ok(new { success = true });
    }

    [Authorize]
    [HttpGet("me")]
    public async Task<IActionResult> Me()
    {
        if (!int.TryParse(User.FindFirstValue(ClaimTypes.NameIdentifier), out var userId)) return Unauthorized();
        var user = await _users.GetProfileAsync(userId);
        return user is null ? Unauthorized() : Ok(user);
    }

    [Authorize]
    [HttpPost("profile-image")]
    [RequestSizeLimit(5 * 1024 * 1024)]
    public async Task<IActionResult> UploadProfileImage(IFormFile? file)
    {
        if (!int.TryParse(User.FindFirstValue(ClaimTypes.NameIdentifier), out var userId)) return Unauthorized();
        if (file is null || file.Length == 0)
            return BadRequest(new { success = false, message = "ØªØµÙˆÛŒØ±ÛŒ Ø§Ù†ØªØ®Ø§Ø¨ Ù†Ø´Ø¯Ù‡ Ø§Ø³Øª." });
        if (file.Length > 5 * 1024 * 1024)
            return BadRequest(new { success = false, message = "Ø­Ø¬Ù… ØªØµÙˆÛŒØ± Ù†Ø¨Ø§ÛŒØ¯ Ø¨ÛŒØ´ØªØ± Ø§Ø² ۵ Ù…Ú¯Ø§Ø¨Ø§ÛŒØª Ø¨Ø§Ø´Ø¯." });

        var extension = file.ContentType.ToLowerInvariant() switch
        {
            "image/jpeg" => ".jpg",
            "image/png" => ".png",
            "image/webp" => ".webp",
            "image/gif" => ".gif",
            _ => null
        };
        if (extension is null)
            return BadRequest(new { success = false, message = "ÙØ±Ù…Øª ØªØµÙˆÛŒØ± Ø¨Ø§ÛŒØ¯ JPGØŒ PNGØŒ WEBP ÛŒØ§ GIF Ø¨Ø§Ø´Ø¯." });

        if (extension is not null && !HasValidImageSignature(file, extension))
            return BadRequest(new { success = false, message = "فایل انتخاب‌شده یک تصویر معتبر نیست." });

        var user = await _db.Users.SingleOrDefaultAsync(x => x.Id == userId && x.IsActive);
        if (user is null) return Unauthorized();

        var webRoot = _environment.WebRootPath ?? Path.Combine(_environment.ContentRootPath, "wwwroot");
        var uploadDirectory = Path.Combine(webRoot, "uploads", "profiles");
        Directory.CreateDirectory(uploadDirectory);
        var fileName = $"{Guid.NewGuid():N}{extension}";
        var filePath = Path.Combine(uploadDirectory, fileName);
        var profileImageUrl = $"/uploads/profiles/{fileName}";

        try
        {
            await using (var stream = new FileStream(filePath, FileMode.CreateNew, FileAccess.Write, FileShare.None))
            {
                await file.CopyToAsync(stream);
            }

            var previousImageUrl = user.ProfileImageUrl;
            user.ProfileImageUrl = profileImageUrl;
            await _db.SaveChangesAsync();
            try { DeleteStoredProfileImage(previousImageUrl, webRoot); } catch { /* The new image is already stored and remains usable. */ }
            return Ok(new { success = true, profileImageUrl });
        }
        catch
        {
            if (System.IO.File.Exists(filePath)) System.IO.File.Delete(filePath);
            throw;
        }
    }

    [Authorize]
    [HttpPut("me")]
    public async Task<IActionResult> UpdateProfile([FromBody] UpdateUserProfileDto model)
    {
        if (!int.TryParse(User.FindFirstValue(ClaimTypes.NameIdentifier), out var userId)) return Unauthorized();
        if (string.IsNullOrWhiteSpace(model.FirstName) || string.IsNullOrWhiteSpace(model.LastName))
            return BadRequest(new { success = false, message = "نام و نام خانوادگی الزامی است." });

        var email = string.IsNullOrWhiteSpace(model.Email) ? null : model.Email.Trim().ToLowerInvariant();
        if (email is not null && !System.Net.Mail.MailAddress.TryCreate(email, out _))
            return BadRequest(new { success = false, message = "ایمیل معتبر نیست." });
        if (email is not null && await _db.Users.AnyAsync(x => x.Email == email && x.Id != userId))
            return BadRequest(new { success = false, message = "این ایمیل قبلاً استفاده شده است." });
        var nationalCode = NormalizeDigits(model.NationalCode);
        if (nationalCode is not null && (nationalCode.Length != 10 || nationalCode.Any(x => x is < '0' or > '9')))
            return BadRequest(new { success = false, message = "کد ملی باید ۱۰ رقم باشد." });
        if (model.BirthDate > DateOnly.FromDateTime(DateTime.Today))
            return BadRequest(new { success = false, message = "تاریخ تولد نمی‌تواند در آینده باشد." });

        var user = await _db.Users.SingleOrDefaultAsync(x => x.Id == userId && x.IsActive);
        if (user is null) return Unauthorized();

        user.FirstName = model.FirstName.Trim();
        user.LastName = model.LastName.Trim();
        user.Email = email;
        user.BirthDate = model.BirthDate;
        user.NationalCode = nationalCode;
        await _db.SaveChangesAsync();
        return Ok(new { success = true, user = new { user.FirstName, user.LastName, user.Email, user.PhoneNumber, user.BirthDate, user.NationalCode, user.ProfileImageUrl } });
    }

    private static void DeleteStoredProfileImage(string? imageUrl, string webRoot)
    {
        if (string.IsNullOrWhiteSpace(imageUrl) || !imageUrl.StartsWith("/uploads/profiles/", StringComparison.OrdinalIgnoreCase)) return;
        var fileName = Path.GetFileName(imageUrl);
        if (string.IsNullOrWhiteSpace(fileName)) return;
        var path = Path.Combine(webRoot, "uploads", "profiles", fileName);
        if (System.IO.File.Exists(path)) System.IO.File.Delete(path);
    }

    private static bool HasValidImageSignature(IFormFile file, string extension)
    {
        Span<byte> header = stackalloc byte[12];
        using var stream = file.OpenReadStream();
        var bytesRead = stream.Read(header);
        return extension switch
        {
            ".jpg" => bytesRead >= 3 && header[0] == 0xFF && header[1] == 0xD8 && header[2] == 0xFF,
            ".png" => bytesRead >= 8 && header[..8].SequenceEqual(new byte[] { 0x89, 0x50, 0x4E, 0x47, 0x0D, 0x0A, 0x1A, 0x0A }),
            ".gif" => bytesRead >= 6 && (header[..6].SequenceEqual("GIF87a"u8) || header[..6].SequenceEqual("GIF89a"u8)),
            ".webp" => bytesRead >= 12 && header[..4].SequenceEqual("RIFF"u8) && header[8..12].SequenceEqual("WEBP"u8),
            _ => false
        };
    }

    private static string? NormalizeDigits(string? value)
    {
        if (string.IsNullOrWhiteSpace(value)) return null;
        return new string(value.Trim().Select(character => character switch
        {
            >= '۰' and <= '۹' => (char)('0' + character - '۰'),
            >= '٠' and <= '٩' => (char)('0' + character - '٠'),
            _ => character
        }).ToArray());
    }

    private Task SignInAsync(UserProfileDto user, bool isPersistent)
    {
        var claims = new[]
        {
            new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
            new Claim(ClaimTypes.Name, user.FullName),
            new Claim(ClaimTypes.MobilePhone, user.PhoneNumber),
            new Claim(ClaimTypes.Role, user.Role)
        };
        var principal = new ClaimsPrincipal(new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme));
        var customerKeyOptions = new CookieOptions { HttpOnly = true, IsEssential = true, SameSite = SameSiteMode.Lax, Path = "/" };
        if (isPersistent)
        {
            customerKeyOptions.Expires = DateTimeOffset.UtcNow.AddDays(14);
            customerKeyOptions.MaxAge = TimeSpan.FromDays(14);
        }
        Response.Cookies.Append("customer-key", $"user-{user.Id}", customerKeyOptions);
        var now = DateTimeOffset.UtcNow;
        return HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, principal,
            new AuthenticationProperties
            {
                IsPersistent = isPersistent,
                AllowRefresh = isPersistent,
                IssuedUtc = now,
                ExpiresUtc = isPersistent ? now.AddDays(14) : null
            });
    }
}
