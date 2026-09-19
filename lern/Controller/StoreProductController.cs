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
        return View(StoreProductDetailsViewModel.FromProduct(product, relatedProducts));
    }
}
