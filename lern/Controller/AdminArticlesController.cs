using Entities;
using lern.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace lern.Controller;

[Authorize(Roles = "Admin")]
[Route("Admin/Articles")]
[ResponseCache(Location = ResponseCacheLocation.None, NoStore = true)]
public class AdminArticlesController(ShopDbContext db, IWebHostEnvironment environment) : Microsoft.AspNetCore.Mvc.Controller
{
    [HttpGet("")]
    public async Task<IActionResult> Index() => View("~/Views/Admin/Articles/Index.cshtml",
        await db.Articles.AsNoTracking().OrderByDescending(x => x.CreatedAt).ToListAsync());

    [HttpGet("create")]
    public IActionResult Create() => View("~/Views/Admin/Articles/Form.cshtml", new ArticleForm());

    [HttpGet("{id:int}/edit")]
    public async Task<IActionResult> Edit(int id)
    {
        var article = await db.Articles.FindAsync(id);
        if (article is null) return NotFound();
        return View("~/Views/Admin/Articles/Form.cshtml", new ArticleForm
        {
            Id = id, Title = article.Title, Slug = article.Slug, Summary = article.Summary,
            Content = lern.Infrastructure.RichText.Render(article.Content), Category = article.Category, Author = article.Author,
            ImageAlt = article.ImageAlt, ExistingImageUrl = article.ImageUrl, IsPublished = article.IsPublished
        });
    }

    [HttpPost("create")]
    [HttpPost("{id:int}/edit")]
    [ValidateAntiForgeryToken]
    [RequestSizeLimit(6 * 1024 * 1024)]
    public async Task<IActionResult> Save(int? id, ArticleForm form)
    {
        var article = id.HasValue ? await db.Articles.FindAsync(id.Value) : new Article();
        if (article is null) return NotFound();
        form.Id = id; form.ExistingImageUrl = article.ImageUrl;
        form.Slug = form.Slug?.Trim() ?? "";
        if (await db.Articles.AnyAsync(x => x.Slug == form.Slug && x.Id != (id ?? 0)))
            ModelState.AddModelError(nameof(form.Slug), "این نشانی قبلاً استفاده شده است.");
        if (!lern.Infrastructure.RichText.HasText(form.Content)) ModelState.AddModelError(nameof(form.Content), "متن مقاله را وارد کنید.");
        if (!id.HasValue && form.Image is not { Length: > 0 })
            ModelState.AddModelError(nameof(form.Image), "تصویر اختصاصی مقاله را انتخاب کنید.");
        string? saved = null;
        if (form.Image is { Length: > 0 } image)
        {
            var extension = Path.GetExtension(image.FileName).ToLowerInvariant();
            if (image.Length > 5 * 1024 * 1024 || extension is not (".jpg" or ".jpeg" or ".png") || !await IsImage(image, extension))
                ModelState.AddModelError(nameof(form.Image), "تصویر باید JPG یا PNG معتبر و حداکثر ۵ مگابایت باشد.");
        }
        if (!ModelState.IsValid) return View("~/Views/Admin/Articles/Form.cshtml", form);
        if (form.Image is { Length: > 0 } upload)
        {
            var directory = Path.Combine(environment.WebRootPath, "uploads", "articles");
            Directory.CreateDirectory(directory);
            saved = "/uploads/articles/" + Guid.NewGuid().ToString("N") + Path.GetExtension(upload.FileName).ToLowerInvariant();
            await using var stream = System.IO.File.Create(Path.Combine(environment.WebRootPath, saved.TrimStart('/').Replace('/', Path.DirectorySeparatorChar)));
            await upload.CopyToAsync(stream);
        }
        article.Title = form.Title.Trim(); article.Slug = form.Slug;
        article.Summary = form.Summary.Trim(); article.Content = lern.Infrastructure.RichText.Render(form.Content);
        article.Category = form.Category.Trim(); article.Author = form.Author.Trim();
        article.ImageAlt = form.ImageAlt.Trim(); article.ImageUrl = saved ?? article.ImageUrl;
        if (form.IsPublished && article.PublishedAt is null) article.PublishedAt = DateTime.UtcNow;
        article.IsPublished = form.IsPublished;
        if (!id.HasValue) db.Articles.Add(article);
        try { await db.SaveChangesAsync(); }
        catch
        {
            if (saved is not null) System.IO.File.Delete(Path.Combine(environment.WebRootPath, saved.TrimStart('/').Replace('/', Path.DirectorySeparatorChar)));
            throw;
        }
        TempData["ArticleMessage"] = "مقاله ذخیره شد.";
        return RedirectToAction(nameof(Index));
    }

    private static async Task<bool> IsImage(IFormFile image, string extension)
    {
        var bytes = new byte[24];
        await using var stream = image.OpenReadStream();
        if (await stream.ReadAsync(bytes.AsMemory()) < bytes.Length) return false;
        if (extension == ".png")
            return bytes.AsSpan(0, 8).SequenceEqual(new byte[] { 137, 80, 78, 71, 13, 10, 26, 10 }) &&
                bytes.AsSpan(12, 4).SequenceEqual("IHDR"u8) &&
                System.Buffers.Binary.BinaryPrimitives.ReadInt32BigEndian(bytes.AsSpan(16, 4)) > 0 &&
                System.Buffers.Binary.BinaryPrimitives.ReadInt32BigEndian(bytes.AsSpan(20, 4)) > 0;
        return bytes[0] == 255 && bytes[1] == 216 && bytes[2] == 255;
    }
}
