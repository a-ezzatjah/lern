using Entities;
using lern.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ServiceContract.Interfaces;

namespace lern.Controller;

[Route("shop")]
public sealed class ShopController(ShopDbContext db, IProductService products) : Microsoft.AspNetCore.Mvc.Controller
{
    [HttpGet("")]
    public async Task<IActionResult> Index(string? q, [FromQuery(Name = "category")] int[]? categories,
        bool available = false, string sort = "newest", decimal? minPrice = null, decimal? maxPrice = null,
        int offset = 0, int take = 6, bool append = false, bool discounted = false,
        [FromQuery(Name = "color")] string[]? colors = null)
    {
        q = q?.Trim();
        if (q?.Length > 100) q = q[..100];
        if (sort is not ("newest" or "bestselling" or "popular" or "price_desc" or "price_asc")) sort = "newest";
        var allCategories = await db.Categories.AsNoTracking().OrderBy(c => c.SortOrder).ThenBy(c => c.Name).ToListAsync();
        var byId = allCategories.ToDictionary(c => c.Id);
        var selected = (categories ?? []).Where(byId.ContainsKey).Distinct().Take(30).ToHashSet();
        var expanded = new HashSet<int>(selected);
        var trail = new List<Category>();
        if (selected.Count == 1)
        {
            var current = selected.Single();
            var visited = new HashSet<int>();
            while (visited.Add(current) && byId.TryGetValue(current, out var node))
            {
                trail.Insert(0, node);
                if (!node.ParentId.HasValue) break;
                current = node.ParentId.Value;
            }
        }
        foreach (var id in selected)
        {
            var current = id;
            while (byId.TryGetValue(current, out var node) && node.ParentId.HasValue && expanded.Add(node.ParentId.Value))
                current = node.ParentId.Value;
        }
        ViewData["ExpandedCategories"] = expanded;
        minPrice = minPrice.HasValue && minPrice >= 0 ? minPrice : null;
        maxPrice = maxPrice.HasValue && maxPrice >= 0 ? maxPrice : null;
        take = append ? Math.Clamp(take, 1, 3) : 6;
        offset = append ? Math.Max(0, offset) : 0;
        var result = await products.GetShopProductCardsAsync(q, selected.ToArray(), available, sort, minPrice, maxPrice, offset, take, discounted,
            (colors ?? []).Where(c => !string.IsNullOrWhiteSpace(c) && c.Length <= 50).Distinct().Take(50).ToArray());
        ViewData["Title"] = string.IsNullOrWhiteSpace(q) ? (discounted ? "محصولات تخفیف‌دار" : "محصولات") : $"نتایج جستجوی {q}";
        if (selected.Count == 1)
        {
            var category = byId[selected.Single()];
            ViewData["Title"] = string.IsNullOrWhiteSpace(category.Seo?.MetaTitle) ? category.Name : category.Seo.MetaTitle;
            var description = string.IsNullOrWhiteSpace(category.Seo?.MetaDescription)
                ? lern.Infrastructure.RichText.Summary(category.Description) : category.Seo.MetaDescription;
            ViewData["MetaDescription"] = string.IsNullOrWhiteSpace(description) ? null : description;
            ViewData["CanonicalUrl"] = category.Seo?.CanonicalUrl;
            ViewData["Robots"] = $"{(category.Seo?.IndexPage ?? true ? "index" : "noindex")}, {(category.Seo?.FollowPage ?? true ? "follow" : "nofollow")}";
        }
        var model = new ShopViewModel(result, allCategories, trail, q ?? "", selected, available, sort, minPrice, maxPrice, discounted)
        { Colors = result.Colors, SelectedColors = result.SelectedColors };
        if (Request.Headers["X-Requested-With"] == "XMLHttpRequest")
            return append ? PartialView("~/Views/Shop/_Cards.cshtml", result.Items)
                : PartialView("~/Views/Shop/_Ajax.cshtml", model);
        return View(model);
    }

    [HttpGet("suggest")]
    public async Task<IActionResult> Suggest(string? q)
    {
        q = q?.Trim();
        if (string.IsNullOrWhiteSpace(q) || (q.Length < 2 && !char.IsDigit(q[0])))
            return Ok(new { items = Array.Empty<object>(), hasMore = false });
        if (q.Length > 100) q = q[..100];
        var results = await products.GetShopProductCardsAsync(q, [], false, "newest", null, null, 0, 10);
        return Ok(new { items = results.Items.Select(p => new { p.Id, p.Name, p.PrimaryImageUrl }), hasMore = results.TotalCount > results.Items.Count });
    }
}
