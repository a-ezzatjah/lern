using System.Data;
using Entities;
using Microsoft.EntityFrameworkCore;

namespace lern.Infrastructure;

public static class InitialAdminSetup
{
    public static async Task<string> PromoteAsync(ShopDbContext db, string phoneNumber)
    {
        var phone = NormalizePhone(phoneNumber);
        if (phone is null) throw new InvalidOperationException("شمارهٔ مدیر باید یک شمارهٔ موبایل معتبر باشد.");
        await using var transaction = db.Database.CurrentTransaction is null
            ? await db.Database.BeginTransactionAsync(IsolationLevel.Serializable) : null;
        if (await db.Users.AnyAsync(x => x.Role == "Admin"))
            return "مدیر از قبل تعیین شده است؛ هیچ حسابی تغییر نکرد.";
        var user = await db.Users.SingleOrDefaultAsync(x => x.PhoneNumber == phone && x.IsActive);
        if (user is null) throw new InvalidOperationException("حساب فعال این شماره پیدا نشد؛ ابتدا در سایت ثبت‌نام کنید و سپس راه‌اندازی مدیر را اجرا کنید.");
        user.Role = "Admin";
        await db.SaveChangesAsync();
        if (transaction is not null) await transaction.CommitAsync();
        return "اولین مدیر تعیین شد. تنظیم InitialAdmin را حذف کنید و دوباره وارد حساب شوید.";
    }

    private static string? NormalizePhone(string? input)
    {
        if (string.IsNullOrWhiteSpace(input)) return null;
        var value = input.Trim();
        value = new string(value.Select(c => c switch
        {
            >= '۰' and <= '۹' => (char)('0' + c - '۰'),
            >= '٠' and <= '٩' => (char)('0' + c - '٠'),
            _ => c
        }).ToArray());
        if (value.StartsWith("+98")) value = "0" + value[3..];
        else if (value.StartsWith("0098")) value = "0" + value[4..];
        return value.Length == 11 && value.StartsWith("09") && value.All(char.IsAsciiDigit) ? value : null;
    }
}
