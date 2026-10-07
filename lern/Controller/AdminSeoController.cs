using Entities;
using lern.Infrastructure;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace lern.Controller;

[Authorize(Roles = "Admin")]
[Route("Admin/Seo")]
[ResponseCache(Location = ResponseCacheLocation.None, NoStore = true)]
public class AdminSeoController(ShopDbContext db, SeoCatalog catalog) : Microsoft.AspNetCore.Mvc.Controller
{
    [HttpGet("")]
    public async Task<IActionResult> Index(string? q = null)
    {
        if (q?.Length > 100) return BadRequest();
        ViewData["SeoQuery"] = q?.Trim() ?? "";
        var nodes = AdminSeoTree.Filter(await new AdminSeoTree(db, catalog).Build(), q);
        if (Request.Headers["X-Requested-With"] == "XMLHttpRequest")
            return PartialView("~/Views/Admin/Seo/_Tree.cshtml", nodes);
        return View("~/Views/Admin/Seo/Index.cshtml", nodes);
    }

    [HttpGet("children")]
    public async Task<IActionResult> Children(string key, int offset = 0, string? q = null)
    {
        if (string.IsNullOrWhiteSpace(key) || key.Length > 200 || q?.Length > 100 || offset < 0 || offset > int.MaxValue - AdminSeoTree.PageSize) return BadRequest();
        ViewData["SeoQuery"] = q?.Trim() ?? "";
        var node = AdminSeoTree.Find(AdminSeoTree.Filter(await new AdminSeoTree(db, catalog).Build(), q), key);
        if (node is null) return NotFound();
        return PartialView("~/Views/Admin/Seo/_Children.cshtml", AdminSeoTree.Children(node, offset));
    }

    [HttpPost("")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Save(SeoPage model)
    {
        if (!ModelState.IsValid || !(await catalog.Entries()).Any(x => x.Path == model.Path)) return BadRequest();
        var entry = await db.SeoPages.FindAsync(model.Path);
        if (entry is null) { entry = new SeoPage { Path = model.Path }; db.SeoPages.Add(entry); }
        entry.MetaTitle = model.MetaTitle?.Trim(); entry.MetaDescription = model.MetaDescription?.Trim();
        entry.IndexPage = model.IndexPage; entry.FollowPage = model.FollowPage; entry.IncludeInSitemap = model.IncludeInSitemap;
        // Keep existing product/category forms consistent with sitemap settings.
        SeoData? seo = null;
        if (model.Path.StartsWith("/product/") && int.TryParse(model.Path[9..], out var productId))
        {
            var product = await db.Products.FindAsync(productId);
            if (product is not null) seo = product.Seo ??= new SeoData();
        }
        if (model.Path.StartsWith("/shop?category=") && int.TryParse(model.Path[15..], out var categoryId))
        {
            var category = await db.Categories.FindAsync(categoryId);
            if (category is not null) seo = category.Seo ??= new SeoData();
        }
        if (seo is not null)
        {
            seo.IndexPage = model.IndexPage; seo.FollowPage = model.FollowPage;
            seo.MetaTitle = entry.MetaTitle; seo.MetaDescription = entry.MetaDescription;
        }
        await db.SaveChangesAsync();
        if (HttpContext is not null && Request.Headers["X-Requested-With"] == "XMLHttpRequest")
        {
            var saved = (await catalog.Entries()).Single(x => x.Path == model.Path);
            return Json(new { success = true, message = "تنظیمات سئو و نقشه سایت ذخیره شد.",
                indexPage = saved.IndexPage, followPage = saved.FollowPage, includeInSitemap = saved.IncludeInSitemap,
                isInSitemap = saved.IsInSitemap, metaTitle = saved.MetaTitle, metaDescription = saved.MetaDescription });
        }
        TempData["SeoMessage"] = "تنظیمات سئو و نقشه سایت ذخیره شد.";
        return RedirectToAction(nameof(Index));
    }

    [HttpGet("/Admin/Settings/category-seo")]
    public async Task<IActionResult> Categories(string? q = null)
    {
        if (q?.Length > 100) return BadRequest();
        ViewData["SeoQuery"] = q?.Trim() ?? "";
        var nodes = AdminSeoTree.Filter(await new AdminCategorySeoTree(db).Build(), q);
        if (Request.Headers["X-Requested-With"] == "XMLHttpRequest")
            return PartialView("~/Views/Admin/Seo/_CategorySeoTree.cshtml", nodes);
        return View("~/Views/Admin/Seo/Categories.cshtml", nodes);
    }

    [HttpGet("/Admin/Settings/category-seo/children")]
    public async Task<IActionResult> CategoryChildren(string key, int offset = 0, string? q = null)
    {
        if (string.IsNullOrWhiteSpace(key) || key.Length > 200 || q?.Length > 100 || offset < 0 || offset > int.MaxValue - AdminSeoTree.PageSize) return BadRequest();
        ViewData["SeoQuery"] = q?.Trim() ?? "";
        var node = AdminSeoTree.Find(AdminSeoTree.Filter(await new AdminCategorySeoTree(db).Build(), q), key);
        if (node is null) return NotFound();
        return PartialView("~/Views/Admin/Seo/_CategorySeoChildren.cshtml", AdminSeoTree.Children(node, offset));
    }

    [HttpGet("/Admin/Settings/category-seo/{id:int}/editor")]
    public async Task<IActionResult> CategoryEditor(int id)
    {
        var category = await db.Categories.AsNoTracking().SingleOrDefaultAsync(x => x.Id == id);
        if (category is null) return NotFound();
        var include = await db.SeoPages.AsNoTracking().Where(x => x.Path == $"/shop?category={id}")
            .Select(x => (bool?)x.IncludeInSitemap).SingleOrDefaultAsync() ?? true;
        return PartialView("~/Views/Admin/Seo/_CategorySeoEditor.cshtml", new lern.Models.CategorySeoEditorViewModel(category, include));
    }

    [HttpPost("/Admin/Settings/category-seo/{id:int}")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> SaveCategory(int id, string? description, string? metaTitle, string? metaDescription,
        bool? indexPage = null, bool? followPage = null, bool? includeInSitemap = null)
    {
        if (!ModelState.IsValid) return BadRequest();
        var category = await db.Categories.FindAsync(id);
        if (category is null) return NotFound();
        if (description?.Length > 100000 || metaTitle?.Length > 200 || metaDescription?.Length > 500) return BadRequest();
        category.Description = RichText.Render(description);
        category.Seo ??= new SeoData();
        category.Seo.MetaTitle = metaTitle?.Trim(); category.Seo.MetaDescription = metaDescription?.Trim();
        if (indexPage.HasValue) category.Seo.IndexPage = indexPage.Value;
        if (followPage.HasValue) category.Seo.FollowPage = followPage.Value;
        var path = $"/shop?category={id}";
        var page = await db.SeoPages.FindAsync(path);
        if (includeInSitemap.HasValue)
        {
            if (page is null) { page = new SeoPage { Path = path }; db.SeoPages.Add(page); }
            page.IncludeInSitemap = includeInSitemap.Value;
        }
        await db.SaveChangesAsync();
        if (HttpContext is not null && Request.Headers["X-Requested-With"] == "XMLHttpRequest")
        {
            var entry = new lern.Models.CategorySeoEditorViewModel(category, page?.IncludeInSitemap ?? true).Entry;
            return Json(new { success = true, message = "توضیحات و سئوی دسته‌بندی ذخیره شد.",
                indexPage = entry.IndexPage, followPage = entry.FollowPage, includeInSitemap = entry.IncludeInSitemap,
                isInSitemap = entry.IsInSitemap, metaTitle = entry.MetaTitle, metaDescription = entry.MetaDescription, description = category.Description });
        }
        TempData["SeoMessage"] = "توضیحات دسته‌بندی ذخیره شد.";
        return RedirectToAction(nameof(Categories));
    }
}
