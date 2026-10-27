using System.IO.Compression;
using System.Text.Json;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Entities;

namespace lern.Controller;

[Authorize(Roles = "Admin")]
[Route("Admin/StoreImport")]
public sealed class StoreImportController(ShopDbContext db) : Microsoft.AspNetCore.Mvc.Controller
{
    [HttpPost]
    [RequestSizeLimit(100_000_000)]
    public async Task<IActionResult> Import(IFormFile? package, CancellationToken ct)
    {
        if (package is null || !package.FileName.EndsWith(".zip", StringComparison.OrdinalIgnoreCase)) return BadRequest("Upload a .zip package.");
        await using var input = package.OpenReadStream();
        using var zip = new ZipArchive(input, ZipArchiveMode.Read);
        var entry = zip.GetEntry("data.json");
        if (entry is null) return BadRequest("data.json is missing.");
        await using var stream = entry.Open();
        var model = await JsonSerializer.DeserializeAsync<ImportPackage>(stream, cancellationToken: ct);
        if (model is null) return BadRequest("Invalid data.json.");
        var result = new ImportResult();
        foreach (var item in model.Products ?? [])
        {
            var entity = await db.Products.SingleOrDefaultAsync(x => x.Slug == item.Slug, ct);
            if (entity is null) { entity = new Product { Slug = item.Slug, CreatedAt = DateTime.UtcNow }; db.Products.Add(entity); result.ProductsAdded++; } else result.ProductsUpdated++;
            entity.Name=item.Name; entity.Description=item.Description; entity.ShortDescription=item.ShortDescription; entity.IsActive=item.IsActive; entity.UpdatedAt=DateTime.UtcNow;
        }
        foreach (var item in model.Articles ?? [])
        {
            var entity = await db.Articles.SingleOrDefaultAsync(x => x.Slug == item.Slug, ct);
            if (entity is null) { entity = new Article { Slug = item.Slug, CreatedAt = DateTime.UtcNow }; db.Articles.Add(entity); result.ArticlesAdded++; } else result.ArticlesUpdated++;
            entity.Title=item.Title; entity.Summary=item.Summary; entity.Content=item.Content; entity.Category=item.Category; entity.Author=item.Author; entity.ImageUrl=item.ImageUrl; entity.ImageAlt=item.ImageAlt; entity.IsPublished=item.IsPublished; entity.PublishedAt=item.PublishedAt;
        }
        foreach (var item in model.Banners ?? [])
        {
            var entity = item.Id > 0 ? await db.SiteBanners.FindAsync([item.Id], ct) : null;
            if (entity is null) { entity = new SiteBanner(); db.SiteBanners.Add(entity); result.BannersAdded++; } else result.BannersUpdated++;
            entity.ImageUrl=item.ImageUrl; entity.MobileImageUrl=item.MobileImageUrl; entity.LinkUrl=item.LinkUrl; entity.AltText=item.AltText; entity.SortOrder=item.SortOrder; entity.IsActive=item.IsActive;
        }
        await db.SaveChangesAsync(ct);
        return Ok(result);
    }
    public sealed class ImportPackage { public List<ProductItem>? Products {get;set;} public List<ArticleItem>? Articles {get;set;} public List<BannerItem>? Banners {get;set;} }
    public sealed class ProductItem { public string Name {get;set;}=""; public string Slug {get;set;}=""; public string? Description {get;set;} public string? ShortDescription {get;set;} public bool IsActive {get;set;}=true; }
    public sealed class ArticleItem { public string Slug {get;set;}=""; public string Title {get;set;}=""; public string Summary {get;set;}=""; public string Content {get;set;}=""; public string Category {get;set;}=""; public string Author {get;set;}=""; public string ImageUrl {get;set;}=""; public string ImageAlt {get;set;}=""; public bool IsPublished {get;set;} public DateTime? PublishedAt {get;set;} }
    public sealed class BannerItem { public int Id {get;set;} public string ImageUrl {get;set;}=""; public string? MobileImageUrl {get;set;} public string? LinkUrl {get;set;} public string AltText {get;set;}=""; public int SortOrder {get;set;} public bool IsActive {get;set;}=true; }
    public sealed class ImportResult { public int ProductsAdded {get;set;} public int ProductsUpdated {get;set;} public int ArticlesAdded {get;set;} public int ArticlesUpdated {get;set;} public int BannersAdded {get;set;} public int BannersUpdated {get;set;} }
}
