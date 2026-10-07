using Entities;
using lern.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace lern.Controller;

[Route("articles")]
public class ArticlesController(ShopDbContext db) : Microsoft.AspNetCore.Mvc.Controller
{
    [HttpGet("")]
    [HttpGet("/blog")]
    public async Task<IActionResult> Index(string? q, string? category, int page = 1)
    {
        q = q?.Trim();
        if (q?.Length > 100 || category?.Length > 80) return BadRequest();
        var query = db.Articles.AsNoTracking().Where(x => x.IsPublished);
        if (!string.IsNullOrEmpty(q)) query = query.Where(x => x.Title.Contains(q) || x.Summary.Contains(q));
        if (!string.IsNullOrEmpty(category)) query = query.Where(x => x.Category == category);
        var model = await Sidebar();
        model.TotalPages = Math.Max(1, (int)Math.Ceiling(await query.CountAsync() / 9d));
        model.Page = Math.Clamp(page, 1, model.TotalPages);
        model.Query = q; model.Category = category;
        model.Articles = await query.OrderByDescending(x => x.PublishedAt).ThenByDescending(x => x.Id)
            .Skip((model.Page - 1) * 9).Take(9).ToListAsync();
        return View(model);
    }

    [HttpGet("{slug}")]
    public async Task<IActionResult> Details(string slug)
    {
        var article = await db.Articles.AsNoTracking().SingleOrDefaultAsync(x => x.Slug == slug && x.IsPublished);
        if (article is null) return NotFound();
        return View(new ArticleDetailsViewModel { Article = article, Sidebar = await Sidebar() });
    }

    private async Task<ArticleListViewModel> Sidebar() => new()
    {
        Latest = await db.Articles.AsNoTracking().Where(x => x.IsPublished).OrderByDescending(x => x.PublishedAt).ThenByDescending(x => x.Id).Take(3).ToListAsync(),
        Categories = await db.Articles.AsNoTracking().Where(x => x.IsPublished).GroupBy(x => x.Category)
            .Select(x => new { Name = x.Key, Count = x.Count() }).ToDictionaryAsync(x => x.Name, x => x.Count)
    };
}
