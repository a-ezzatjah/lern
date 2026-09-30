using Entities;
using System.Data;
using lern.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace lern.Controller;

[Authorize(Roles = "Admin")]
[Route("Admin/Settings")]
[ResponseCache(Location = ResponseCacheLocation.None, NoStore = true)]
public sealed class AdminSettingsController(ShopDbContext db, IWebHostEnvironment environment) : Microsoft.AspNetCore.Mvc.Controller
{
    [HttpGet("")]
    public IActionResult Index() => RedirectToAction(nameof(Banners));

    [HttpGet("banners")]
    public async Task<IActionResult> Banners() => View("~/Views/Admin/Settings/Banners.cshtml", new AdminSettingsViewModel
    {
        Banners = await db.SiteBanners.AsNoTracking().OrderBy(x => x.SortOrder).ToListAsync()
    });

    [HttpGet("inbox")]
    public async Task<IActionResult> Inbox() => View("~/Views/Admin/Settings/Inbox.cshtml", new AdminSettingsViewModel
    {
        Inbox = await db.SiteInboxItems.AsNoTracking().Include(x => x.RecipientUser)
            .OrderByDescending(x => x.CreatedAt).Take(20).ToListAsync()
    });

    [HttpPost("banner/{id:int}")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Banner(int id, IFormFile? image, IFormFile? mobileImage, string? altText, string? linkUrl, bool isActive)
    {
        var banner = await db.SiteBanners.FindAsync(id);
        if (banner is null) return NotFound();
        altText = altText?.Trim();
        linkUrl = linkUrl?.Trim();
        if (string.IsNullOrWhiteSpace(altText) || altText.Length > 150 ||
            (linkUrl is not null && (linkUrl.Length > 400 || !linkUrl.StartsWith('/') || linkUrl.StartsWith("//") ||
                linkUrl.Any(c => c == '\\' || char.IsControl(c)))))
        {
            TempData["SettingsError"] = "متن جایگزین یا لینک داخلی بنر معتبر نیست.";
            return RedirectToAction(nameof(Banners));
        }
        string? savedPath = null;
        string? savedMobilePath = null;
        if (image is { Length: > 0 })
        {
            var ext = Path.GetExtension(image.FileName).ToLowerInvariant();
            if (image.Length > 5 * 1024 * 1024 || ext is not (".jpg" or ".jpeg" or ".png") ||
                !await HasBannerDimensions(image, ext, mobile: false))
            {
                TempData["SettingsError"] = "تصویر بنر باید JPG یا PNG، حداکثر ۵ مگابایت و دقیقاً ۲۰۴۸×۴۲۷ یا ۲۵۶۰×۵۳۳ پیکسل باشد.";
                return RedirectToAction(nameof(Banners));
            }
        }
        if (mobileImage is { Length: > 0 })
        {
            var ext = Path.GetExtension(mobileImage.FileName).ToLowerInvariant();
            if (mobileImage.Length > 5 * 1024 * 1024 || ext is not (".jpg" or ".jpeg" or ".png") ||
                !await HasBannerDimensions(mobileImage, ext, mobile: true))
            {
                TempData["SettingsError"] = "بنر موبایل باید JPG یا PNG، حداکثر ۵ مگابایت و دقیقاً ۸۰۰×۱۰۰۰ پیکسل باشد.";
                return RedirectToAction(nameof(Banners));
            }
        }
        if (image is { Length: > 0 }) savedPath = await SaveBannerImage(image);
        try
        {
            if (mobileImage is { Length: > 0 }) savedMobilePath = await SaveBannerImage(mobileImage);
        }
        catch
        {
            RemoveUploaded(savedPath);
            throw;
        }
        var previous = banner.ImageUrl;
        var previousMobile = banner.MobileImageUrl;
        banner.ImageUrl = savedPath ?? banner.ImageUrl;
        banner.MobileImageUrl = savedMobilePath ?? banner.MobileImageUrl;
        banner.AltText = altText;
        banner.LinkUrl = linkUrl;
        banner.IsActive = isActive;
        try { await db.SaveChangesAsync(); }
        catch
        {
            RemoveUploaded(savedPath);
            RemoveUploaded(savedMobilePath);
            throw;
        }
        if (savedPath is not null) RemoveUploaded(previous);
        if (savedMobilePath is not null) RemoveUploaded(previousMobile);
        TempData["SettingsMessage"] = "بنر صفحهٔ اصلی به‌روزرسانی شد.";
        return RedirectToAction(nameof(Banners));
    }

    [HttpPost("banner/{id:int}/delete")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteBanner(int id)
    {
        var banner = await db.SiteBanners.FindAsync(id);
        if (banner is null) return NotFound();
        db.SiteBanners.Remove(banner);
        await db.SaveChangesAsync();
        RemoveUploaded(banner.ImageUrl);
        RemoveUploaded(banner.MobileImageUrl);
        TempData["SettingsMessage"] = "بنر از صفحهٔ اصلی حذف شد.";
        return RedirectToAction(nameof(Banners));
    }

    [HttpPost("inbox")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Send(SiteInboxKind kind, string? title, string? body, bool sendToAll, int? recipientUserId)
    {
        title = title?.Trim(); body = body?.Trim();
        if (!Enum.IsDefined(kind) || string.IsNullOrWhiteSpace(title) || title.Length > 120 ||
            string.IsNullOrWhiteSpace(body) || body.Length > 1000 ||
            (sendToAll && recipientUserId.HasValue) || (!sendToAll && !recipientUserId.HasValue) ||
            (recipientUserId.HasValue && !await db.Users.AnyAsync(x => x.Id == recipientUserId && x.IsActive)))
        {
            TempData["SettingsError"] = "نوع، متن یا گیرندهٔ پیام معتبر نیست.";
            return RedirectToAction(nameof(Inbox));
        }
        db.SiteInboxItems.Add(new SiteInboxItem { Kind = kind, Title = title, Body = body,
            RecipientUserId = sendToAll ? null : recipientUserId });
        await db.SaveChangesAsync();
        TempData["SettingsMessage"] = "پیام در پنل کاربری گیرنده نمایش داده می‌شود.";
        return RedirectToAction(nameof(Inbox));
    }

    [HttpPost("banner/create")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> CreateBanner(IFormFile? image, IFormFile? mobileImage, string? altText, string? linkUrl)
    {
        if (await db.SiteBanners.CountAsync() >= 8)
        {
            TempData["SettingsError"] = "حداکثر ۸ بنر می‌توان ثبت کرد. برای افزودن بنر جدید، ابتدا یکی از بنرهای موجود را حذف کنید.";
            return RedirectToAction(nameof(Banners));
        }
        if (image is null || image.Length == 0 || string.IsNullOrWhiteSpace(altText))
        {
            TempData["SettingsError"] = "تصویر دسکتاپ و متن جایگزین برای بنر جدید لازم است.";
            return RedirectToAction(nameof(Banners));
        }
        var ext = Path.GetExtension(image.FileName).ToLowerInvariant();
        if (image.Length > 5 * 1024 * 1024 || ext is not (".jpg" or ".jpeg" or ".png") || !await HasBannerDimensions(image, ext, false))
        {
            TempData["SettingsError"] = "ابعاد تصویر دسکتاپ معتبر نیست.";
            return RedirectToAction(nameof(Banners));
        }
        if (mobileImage is { Length: > 0 })
        {
            var mobileExt = Path.GetExtension(mobileImage.FileName).ToLowerInvariant();
            if (mobileImage.Length > 5 * 1024 * 1024 || mobileExt is not (".jpg" or ".jpeg" or ".png") || !await HasBannerDimensions(mobileImage, mobileExt, true))
            {
                TempData["SettingsError"] = "ابعاد تصویر موبایل معتبر نیست.";
                return RedirectToAction(nameof(Banners));
            }
        }
        altText = altText.Trim(); linkUrl = linkUrl?.Trim();
        if (altText.Length > 150 || !IsValidLink(linkUrl))
        {
            TempData["SettingsError"] = "متن جایگزین یا لینک داخلی بنر معتبر نیست.";
            return RedirectToAction(nameof(Banners));
        }
        string? desktop = null, mobile = null;
        try
        {
            desktop = await SaveBannerImage(image);
            if (mobileImage is { Length: > 0 }) mobile = await SaveBannerImage(mobileImage);
            await using var transaction = await db.Database.BeginTransactionAsync(IsolationLevel.Serializable);
            if (await db.SiteBanners.CountAsync() >= 8)
            {
                RemoveUploaded(desktop);
                RemoveUploaded(mobile);
                TempData["SettingsError"] = "ظرفیت ۸ بنر تکمیل شده است. ابتدا یک بنر را حذف کنید.";
                return RedirectToAction(nameof(Banners));
            }
            var lastOrder = await db.SiteBanners.MaxAsync(x => (int?)x.SortOrder) ?? 0;
            db.SiteBanners.Add(new SiteBanner { ImageUrl = desktop, MobileImageUrl = mobile,
                AltText = altText, LinkUrl = linkUrl, SortOrder = lastOrder + 1 });
            await db.SaveChangesAsync();
            await transaction.CommitAsync();
        }
        catch { RemoveUploaded(desktop); RemoveUploaded(mobile); throw; }
        TempData["SettingsMessage"] = "بنر جدید به اسلایدر اضافه شد.";
        return RedirectToAction(nameof(Banners));
    }

    private static bool IsValidLink(string? linkUrl) => linkUrl is null ||
        (linkUrl.Length <= 400 && linkUrl.StartsWith('/') && !linkUrl.StartsWith("//") &&
         !linkUrl.Any(c => c == '\\' || char.IsControl(c)));

    private async Task<string> SaveBannerImage(IFormFile image)
    {
        var folder = Path.Combine(environment.WebRootPath, "uploads", "banners");
        Directory.CreateDirectory(folder);
        var filename = $"{Guid.NewGuid():N}{Path.GetExtension(image.FileName).ToLowerInvariant()}";
        await using var stream = System.IO.File.Create(Path.Combine(folder, filename));
        await image.CopyToAsync(stream);
        return $"/uploads/banners/{filename}";
    }

    private void RemoveUploaded(string? path)
    {
        if (path is null || !path.StartsWith("/uploads/banners/", StringComparison.Ordinal)) return;
        var file = Path.Combine(environment.WebRootPath, "uploads", "banners", Path.GetFileName(path));
        try { if (System.IO.File.Exists(file)) System.IO.File.Delete(file); }
        catch (IOException) { /* Image is no longer referenced. */ }
    }

    private static async Task<bool> HasBannerDimensions(IFormFile image, string extension, bool mobile)
    {
        await using var stream = image.OpenReadStream();
        var bytes = new byte[Math.Min(image.Length, 65536)];
        var read = 0;
        while (read < bytes.Length)
        {
            var count = await stream.ReadAsync(bytes.AsMemory(read));
            if (count == 0) break;
            read += count;
        }
        int width = 0, height = 0;
        if (extension == ".png" && read >= 24 && bytes.AsSpan(0, 8).SequenceEqual(new byte[] { 137, 80, 78, 71, 13, 10, 26, 10 }))
        {
            width = System.Buffers.Binary.BinaryPrimitives.ReadInt32BigEndian(bytes.AsSpan(16, 4));
            height = System.Buffers.Binary.BinaryPrimitives.ReadInt32BigEndian(bytes.AsSpan(20, 4));
        }
        else if (extension is ".jpg" or ".jpeg" && read >= 4 && bytes[0] == 0xff && bytes[1] == 0xd8)
        {
            for (var i = 2; i + 9 < read;)
            {
                if (bytes[i++] != 0xff) break;
                var marker = bytes[i++];
                if (marker is 0xd8 or 0xd9 or 0x01 || marker is >= 0xd0 and <= 0xd7) continue;
                if (i + 2 > read) break;
                var length = (bytes[i] << 8) | bytes[i + 1];
                if (length < 2 || i + length > read) break;
                if (marker is 0xc0 or 0xc1 or 0xc2 or 0xc3)
                {
                    height = (bytes[i + 3] << 8) | bytes[i + 4];
                    width = (bytes[i + 5] << 8) | bytes[i + 6];
                    break;
                }
                i += length;
            }
        }
        return mobile ? width == 800 && height == 1000 :
            (width == 2048 && height == 427) || (width == 2560 && height == 533);
    }
}
