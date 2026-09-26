using System.Security.Claims;
using Entities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using lern.Models;
using ServiceContract.Interfaces;

namespace lern.Controller;

public sealed class AccountController : Microsoft.AspNetCore.Mvc.Controller
{
    private readonly ShopDbContext _db;
    private readonly IProductService _products;

    public AccountController(ShopDbContext db, IProductService products)
    {
        _db = db;
        _products = products;
    }
    [AllowAnonymous]
    [HttpGet("/login")]
    public IActionResult Login() => View();

    [AllowAnonymous]
    [HttpGet("/register")]
    public IActionResult Register() => View();

    [Authorize]
    [HttpGet("/account")]
    public async Task<IActionResult> Index()
    {
        var customerKey = $"user-{User.FindFirstValue(ClaimTypes.NameIdentifier)}";
        var viewedProductIds = await _db.ProductViewHistories.AsNoTracking()
            .Where(x => x.CustomerKey == customerKey)
            .OrderByDescending(x => x.LastViewedAt)
            .Select(x => x.ProductId)
            .Take(8)
            .ToListAsync();
        var productCards = await _products.GetProductCardsAsync();
        var cardsById = productCards.ToDictionary(x => x.Id);
        var recentlyViewedProducts = viewedProductIds
            .Where(cardsById.ContainsKey)
            .Select(id => cardsById[id])
            .ToList();

        return View(new AccountIndexViewModel { RecentlyViewedProducts = recentlyViewedProducts, FavoriteCount = await _db.ProductFavorites.CountAsync(x => x.UserId == int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!) && x.Product.IsActive) });
    }

    [Authorize]
    [HttpGet("/account/profile")]
    public IActionResult Profile() => View();
}
