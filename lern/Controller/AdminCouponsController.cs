using Entities;
using System.Globalization;
using lern.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace lern.Controller;

[Authorize(Roles = "Admin")]
[Route("Admin/Coupons")]
[ResponseCache(Location = ResponseCacheLocation.None, NoStore = true)]
public sealed class AdminCouponsController(ShopDbContext db) : Microsoft.AspNetCore.Mvc.Controller
{
    [HttpGet("")]
    public async Task<IActionResult> Index(int page = 1, int pageSize = 10) =>
        View("~/Views/Admin/Coupons/Index.cshtml", await PageModel(page: page, pageSize: pageSize));

    [HttpGet("create")]
    public async Task<IActionResult> Create() => View("~/Views/Admin/Coupons/Create.cshtml", await PageModel());

    [HttpGet("customers")]
    public async Task<IActionResult> SearchCustomers(string? q)
    {
        q = q?.Trim();
        if (string.IsNullOrWhiteSpace(q) || (q.Length < 2 && !char.IsDigit(q[0]))) return Json(Array.Empty<object>());
        if (q.Length > 100) q = q[..100];
        var normalized = string.Concat(q.Select(c => char.GetNumericValue(c) is >= 0 and <= 9
            ? (char)('0' + (int)char.GetNumericValue(c)) : c));
        int.TryParse(normalized, out var userId);
        var users = await db.Users.AsNoTracking().Where(x => x.IsActive &&
                (x.Id == userId || x.FirstName.Contains(q) || x.LastName.Contains(q) ||
                 (x.FirstName + " " + x.LastName).Contains(q) || x.PhoneNumber.Contains(normalized) ||
                 (x.Email != null && x.Email.Contains(q))))
            .OrderBy(x => x.Id).Take(20)
            .Select(x => new { x.Id, x.FirstName, x.LastName, x.PhoneNumber, x.Email, x.Role }).ToListAsync();
        return Json(users);
    }

    [HttpPost("create")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create([Bind(Prefix = "Form")] AdminCouponCreateModel form)
    {
        form.Code = form.Code?.Trim().ToUpperInvariant() ?? "";
        form.Title = form.Title?.Trim() ?? "";
        if (!Enum.IsDefined(form.Kind)) ModelState.AddModelError("Form.Kind", "نوع تخفیف معتبر نیست.");
        if (form.Kind == CouponKind.Percentage && (form.Value <= 0 || form.Value > 100))
            ModelState.AddModelError("Form.Value", "درصد تخفیف باید بین ۱ تا ۱۰۰ باشد.");
        if (form.Kind == CouponKind.FixedAmount && form.Value <= 0)
            ModelState.AddModelError("Form.Value", "مبلغ تخفیف باید بیشتر از صفر باشد.");
        if (form.Kind == CouponKind.FreeShipping) form.Value = 0;
        DateTime? expiresOn = null;
        var normalizedDate = string.Concat((form.ExpiresOnJalali ?? "").Select(c => char.GetNumericValue(c) is >= 0 and <= 9
            ? (char)('0' + (int)char.GetNumericValue(c)) : c));
        var parts = normalizedDate.Split('/');
        if (parts.Length == 3 && int.TryParse(parts[0], out var year) && int.TryParse(parts[1], out var month) && int.TryParse(parts[2], out var day))
        {
            try { expiresOn = new PersianCalendar().ToDateTime(year, month, day, 0, 0, 0, 0); }
            catch (ArgumentOutOfRangeException) { }
        }
        if (expiresOn is null || expiresOn.Value.Date < AdminOrderDisplay.LocalTime(DateTime.UtcNow).Date)
            ModelState.AddModelError("Form.ExpiresOnJalali", "تاریخ انقضای شمسی معتبر و آینده را وارد کنید.");
        if (form.SendToAll && form.RecipientUserId.HasValue)
            ModelState.AddModelError("Form.RecipientUserId", "برای ارسال همگانی نباید مشتری مشخصی انتخاب شود.");
        if (!form.SendToAll && !form.RecipientUserId.HasValue)
            ModelState.AddModelError("Form.RecipientUserId", "مشتری را جستجو و انتخاب کنید یا ارسال همگانی را فعال کنید.");
        if (form.RecipientUserId.HasValue && !await db.Users.AnyAsync(x => x.Id == form.RecipientUserId && x.IsActive))
            ModelState.AddModelError("Form.RecipientUserId", "مشتری انتخاب‌شده معتبر نیست.");
        if (await db.DiscountCoupons.AnyAsync(x => x.Code == form.Code))
            ModelState.AddModelError("Form.Code", "این کد قبلاً ثبت شده است.");
        if (!ModelState.IsValid)
            return View("~/Views/Admin/Coupons/Create.cshtml", await PageModel(form));

        var expires = TimeZoneInfo.ConvertTimeToUtc(DateTime.SpecifyKind(expiresOn!.Value.Date.AddDays(1), DateTimeKind.Unspecified), AdminOrderDisplay.TimeZone);
        db.DiscountCoupons.Add(new DiscountCoupon
        {
            Title = form.Title, Code = form.Code, Kind = form.Kind, Value = form.Value,
            MinimumOrderAmount = form.MinimumOrderAmount, ExpiresAt = expires,
            RecipientUserId = form.SendToAll ? null : form.RecipientUserId
        });
        await db.SaveChangesAsync();
        TempData["CouponMessage"] = "کارت تخفیف ثبت شد و در پروفایل مشتریان مشمول نمایش داده می‌شود.";
        return RedirectToAction(nameof(Index));
    }

    [HttpPost("{id:int}/toggle")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Toggle(int id, int page = 1, int pageSize = 10)
    {
        var coupon = await db.DiscountCoupons.FindAsync(id);
        if (coupon is null) return NotFound();
        coupon.IsActive = !coupon.IsActive;
        await db.SaveChangesAsync();
        return RedirectToAction(nameof(Index), new { page, pageSize });
    }

    [HttpPost("{id:int}/delete")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Delete(int id, int page = 1, int pageSize = 10)
    {
        var coupon = await db.DiscountCoupons.FindAsync(id);
        if (coupon is null) return NotFound();
        if (await db.CouponRedemptions.AnyAsync(x => x.CouponId == id))
        {
            TempData["CouponError"] = "این کوپن قبلاً در سفارش استفاده شده است و برای حفظ سابقه مالی حذف نمی‌شود؛ می‌توانید آن را بایگانی کنید.";
            return RedirectToAction(nameof(Index), new { page, pageSize });
        }
        db.DiscountCoupons.Remove(coupon);
        try { await db.SaveChangesAsync(); }
        catch (DbUpdateException)
        {
            TempData["CouponError"] = "حذف کوپن انجام نشد. اگر هم‌زمان در سفارشی استفاده شده باشد، آن را بایگانی کنید.";
            return RedirectToAction(nameof(Index), new { page, pageSize });
        }
        TempData["CouponMessage"] = "کوپن حذف شد و دیگر در پروفایل گیرنده نمایش داده نمی‌شود.";
        return RedirectToAction(nameof(Index), new { page, pageSize });
    }

    private async Task<AdminCouponPageModel> PageModel(AdminCouponCreateModel? form = null, int page = 1, int pageSize = 10)
    {
        var count = await db.DiscountCoupons.CountAsync();
        pageSize = new[] { 10, 25, 50 }.Contains(pageSize) ? pageSize : 10;
        page = Math.Clamp(page, 1, Math.Max(1, (int)Math.Ceiling(count / (double)pageSize)));
        return new AdminCouponPageModel
        {
            Form = form ?? new AdminCouponCreateModel(),
            Page = page, PageSize = pageSize, TotalCount = count,
            ActiveCount = await db.DiscountCoupons.CountAsync(x => x.IsActive && x.ExpiresAt > DateTime.UtcNow),
            BroadcastCount = await db.DiscountCoupons.CountAsync(x => x.RecipientUserId == null),
            Coupons = await db.DiscountCoupons.AsNoTracking().Include(x => x.RecipientUser)
                .OrderByDescending(x => x.CreatedAt).ThenByDescending(x => x.Id)
                .Skip((page - 1) * pageSize).Take(pageSize).ToListAsync(),
            Customers = form?.RecipientUserId is int selectedId
            ? await db.Users.AsNoTracking().Where(x => x.Id == selectedId).ToListAsync()
            : new List<CustomerUser>()
        };
    }
}
