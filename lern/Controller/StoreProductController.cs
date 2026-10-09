using System.Security.Claims;
using Entities;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using lern.Models;
using ServiceContract.Interfaces;

namespace lern.Controller;

[Route("product")]
public class StoreProductController : Microsoft.AspNetCore.Mvc.Controller
{
    private readonly IProductService _products;
    private readonly ShopDbContext _db;
    public StoreProductController(IProductService products, ShopDbContext db)
    {
        _products = products;
        _db = db;
    }

    [HttpGet("{id:int}")]
    public async Task<IActionResult> Details(int id)
    {
        var product = await _products.GetPageAsync(id);
        if (product is null)
            return NotFound();

        if (User.Identity?.IsAuthenticated == true)
        {
            var customerKey = $"user-{User.FindFirstValue(ClaimTypes.NameIdentifier)}";
            var viewed = await _db.ProductViewHistories
                .SingleOrDefaultAsync(x => x.CustomerKey == customerKey && x.ProductId == id);
            if (viewed is null)
                _db.ProductViewHistories.Add(new ProductViewHistory { CustomerKey = customerKey, ProductId = id });
            else
                viewed.LastViewedAt = DateTime.UtcNow;

            await _db.SaveChangesAsync();
        }

        var relatedProducts = await _products.GetRelatedProductCardsAsync(id, take: 10);
        var limitedVariants = product.ProductVariants.Where(x => x.MaxPurchaseQuantityPerUser.HasValue).ToList();
        var accountKey = lern.Infrastructure.PurchaseLimitPolicy.AccountKey(User);
        var limitedIds = limitedVariants.Select(x => x.Id).ToArray();
        var purchased = accountKey is null || limitedIds.Length == 0 ? new Dictionary<int, long>() :
            await _db.OrderItems.AsNoTracking().Where(x => limitedIds.Contains(x.ProductVariantId) &&
                x.Order.CustomerKey == accountKey && x.Order.Status != OrderStatus.Cancelled)
                .GroupBy(x => x.ProductVariantId).Select(g => new { Id = g.Key, Quantity = g.Sum(x => (long)x.Quantity) })
                .ToDictionaryAsync(x => x.Id, x => x.Quantity);
        foreach (var variant in limitedVariants)
            variant.RemainingPurchaseQuantity = accountKey is null ? 0 :
                (int)Math.Max(0L, (long)variant.MaxPurchaseQuantityPerUser!.Value - purchased.GetValueOrDefault(variant.Id));
        ViewData["Title"] = product.SeoData?.MetaTitle ?? product.Name;
        ViewData["MetaDescription"] = product.SeoData?.MetaDescription ?? product.ShortDescription;
        ViewData["MetaKeywords"] = product.SeoData?.MetaKeywords;
        ViewData["Robots"] = $"{(product.SeoData?.IndexPage ?? true ? "index" : "noindex")}, {(product.SeoData?.FollowPage ?? true ? "follow" : "nofollow")}";
        ViewData["CanonicalUrl"] = product.SeoData?.CanonicalUrl;
        var categoryCatalog = await _db.Categories.AsNoTracking()
            .Select(c => new Category { Id = c.Id, Name = c.Name, Slug = c.Slug, ParentId = c.ParentId })
            .ToListAsync();
        return View(StoreProductDetailsViewModel.FromProduct(product, relatedProducts, categoryCatalog));
    }
}
