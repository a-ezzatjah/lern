using System.Security.Claims;
using Entities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Data.SqlClient;
using ServiceContract.Interfaces;

namespace lern.Controller;

[Authorize]
public sealed class FavoritesController(ShopDbContext db, IProductService products) : Microsoft.AspNetCore.Mvc.Controller
{
    private int UserId => int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);

    [HttpGet("/account/favorites")]
    public async Task<IActionResult> Index()
    {
        var ids = await db.ProductFavorites.AsNoTracking().Where(x => x.UserId == UserId && x.Product.IsActive)
            .OrderByDescending(x => x.CreatedAt).ThenByDescending(x => x.ProductId).Select(x => x.ProductId).ToListAsync();
        var cards = await products.GetProductCardsAsync();
        var byId = cards.ToDictionary(x => x.Id);
        var categories = await db.ProductCategories.AsNoTracking().Where(x => ids.Contains(x.ProductId))
            .Select(x => new { x.ProductId, x.CategoryId, x.Category.Name }).ToListAsync();
        var variants = await db.ProductVariants.AsNoTracking().Include(x => x.ProductSaleOption)
            .Where(x => ids.Contains(x.ProductSaleOption.ProductId)).ToListAsync();
        var cartOptions = variants.GroupBy(x => x.ProductSaleOption.ProductId)
            .Where(x => x.Count() == 1)
            .Select(x => x.Single())
            .Where(x => x.AvailableQuantity >= Math.Max(1, x.ProductSaleOption.MinQuantity ?? 1))
            .ToDictionary(x => x.ProductSaleOption.ProductId,
                x => new lern.Models.FavoriteCartOption(x.Id, Math.Max(1, x.ProductSaleOption.MinQuantity ?? 1)));
        return View(new lern.Models.FavoritesViewModel {
            CartOptions = cartOptions,
            Products = ids.Where(byId.ContainsKey).Select(id => byId[id]).ToList(),
            Suggested = cards.Where(x => !ids.Contains(x.Id)).Take(4).ToList(),
            Categories = categories.GroupBy(x => x.CategoryId).ToDictionary(x => x.Key, x => x.First().Name),
            ProductCategories = categories.GroupBy(x => x.ProductId).ToDictionary(x => x.Key, x => x.Select(c => c.CategoryId).ToArray())
        });
    }

    [HttpGet("/api/favorites")]
    public async Task<IActionResult> Get() => Ok(await db.ProductFavorites.AsNoTracking()
        .Where(x => x.UserId == UserId && x.Product.IsActive).Select(x => x.ProductId).ToListAsync());

    [HttpPut("/api/favorites/{productId:int}")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Add(int productId)
    {
        if (!await db.Products.AnyAsync(x => x.Id == productId && x.IsActive)) return NotFound();
        if (!await db.ProductFavorites.AnyAsync(x => x.UserId == UserId && x.ProductId == productId))
        {
            db.ProductFavorites.Add(new ProductFavorite { UserId = UserId, ProductId = productId });
            try { await db.SaveChangesAsync(); }
            catch (DbUpdateException ex) when (ex.InnerException is SqlException { Number: 2601 or 2627 }) { }
        }
        return NoContent();
    }

    [HttpDelete("/api/favorites/{productId:int}")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Remove(int productId)
    {
        await db.ProductFavorites.Where(x => x.UserId == UserId && x.ProductId == productId).ExecuteDeleteAsync();
        return NoContent();
    }
}
