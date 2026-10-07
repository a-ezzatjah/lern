using Entities;
using Microsoft.EntityFrameworkCore;

namespace lern.Infrastructure;

public record SeoEntry(string Path, string Title, bool IndexPage = true, bool FollowPage = true, bool IncludeInSitemap = true, string? MetaTitle = null, string? MetaDescription = null)
{
    public bool IsInSitemap => IndexPage && IncludeInSitemap;
}

public class SeoCatalog(ShopDbContext db, IConfiguration config)
{
    public string Origin(HttpRequest request)
    {
        var configured = config["Seo:PublicOrigin"];
        if (Uri.TryCreate(configured, UriKind.Absolute, out var uri) && uri.Scheme is "http" or "https")
            return uri.GetLeftPart(UriPartial.Authority) + request.PathBase;
        return $"{request.Scheme}://{request.Host}{request.PathBase}";
    }

    public async Task<List<SeoEntry>> Entries()
    {
        var result = new List<SeoEntry> { new("/", "صفحه اصلی"), new("/shop", "فروشگاه"), new("/articles", "مقالات") };
        var products = await db.Products.AsNoTracking().Where(x => x.IsActive).OrderBy(x => x.Id)
            .Select(x => new { x.Id, x.Name, x.Seo }).ToListAsync();
        result.AddRange(products.Select(x => new SeoEntry($"/product/{x.Id}", x.Name, x.Seo?.IndexPage ?? true, x.Seo?.FollowPage ?? true,
            string.IsNullOrWhiteSpace(x.Seo?.CanonicalUrl), x.Seo?.MetaTitle, x.Seo?.MetaDescription)));
        var categories = await db.Categories.AsNoTracking().OrderBy(x => x.Id)
            .Select(x => new { x.Id, x.Name, x.Seo }).ToListAsync();
        result.AddRange(categories.Select(x => new SeoEntry($"/shop?category={x.Id}", x.Name, x.Seo?.IndexPage ?? true, x.Seo?.FollowPage ?? true,
            string.IsNullOrWhiteSpace(x.Seo?.CanonicalUrl), x.Seo?.MetaTitle, x.Seo?.MetaDescription)));
        result.AddRange((await db.Articles.AsNoTracking().Where(x => x.IsPublished).OrderBy(x => x.Id)
            .Select(x => new { x.Slug, x.Title, x.Summary }).ToListAsync())
            .Select(x => new SeoEntry("/articles/" + x.Slug, x.Title, MetaDescription: x.Summary)));
        var settings = await db.SeoPages.AsNoTracking().ToDictionaryAsync(x => x.Path);
        return result.Select(x => settings.TryGetValue(x.Path, out var s) && (x.Path.StartsWith("/product/") || x.Path.StartsWith("/shop?category="))
            ? x with { IncludeInSitemap = x.IncludeInSitemap && s.IncludeInSitemap }
            : settings.TryGetValue(x.Path, out s) ? x with {
            IndexPage = s.IndexPage, FollowPage = s.FollowPage, IncludeInSitemap = x.IncludeInSitemap && s.IncludeInSitemap,
            MetaTitle = s.MetaTitle, MetaDescription = s.MetaDescription } : x).ToList();
    }
}
