using Entities;
using Microsoft.EntityFrameworkCore;

namespace lern.Infrastructure;

public static class ArticleSeeder
{
    public static async Task SeedAsync(ShopDbContext db, IWebHostEnvironment environment)
    {
        // Initial demo content only; never recreate articles after an admin edits their slugs.
        if (await db.Articles.AnyAsync()) return;
        var seeds = new[]
        {
            ("sewing-essentials", "راهنمای آماده کردن جعبه خیاطی برای شروع", "آموزش خیاطی", "با ابزارهای ضروری خیاطی آشنا شوید و یک جعبه کاربردی برای نخستین پروژه خود آماده کنید.", "ابزارهای ضروری خیاطی روی میز پارچه‌ای"),
            ("thread-and-needle", "چطور نخ و سوزن مناسب پارچه را انتخاب کنیم؟", "راهنمای خرید", "انتخاب هماهنگ نخ، سوزن و پارچه به دوخت مرتب‌تر کمک می‌کند؛ از شناخت جنس تا آزمایش روی نمونه.", "قرقره‌های نخ و نمونه پارچه برای انتخاب سوزن"),
            ("ribbon-and-lace", "ایده‌های ساده برای تزئین با روبان و توری", "ایده‌های خلاقانه", "با انتخاب رنگ، عرض و روش اتصال مناسب، دست‌سازه‌ها و بسته‌های هدیه را با روبان و توری زیباتر کنید.", "روبان و توری کنار کیسه هدیه پارچه‌ای")
        };
        foreach (var (slug, title, category, summary, alt) in seeds)
        {
            var content = await File.ReadAllTextAsync(Path.Combine(environment.ContentRootPath, "Data", "Articles", slug + ".txt"));
            db.Articles.Add(new Article { Slug = slug, Title = title, Category = category, Summary = summary,
                Content = content, ImageUrl = "/assets/images/articles/" + slug + ".jpg", ImageAlt = alt,
                IsPublished = true, CreatedAt = new DateTime(2026, 10, 7, 8, 0, 0, DateTimeKind.Utc),
                PublishedAt = new DateTime(2026, 10, 7, 8, 0, 0, DateTimeKind.Utc) });
        }
        await db.SaveChangesAsync();
    }
}
