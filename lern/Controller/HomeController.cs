using Microsoft.AspNetCore.Mvc;
using lern.Models;
using ServiceContract.Interfaces;
using Entities;
using Microsoft.EntityFrameworkCore;

namespace lern.Controller;

public class HomeController : Microsoft.AspNetCore.Mvc.Controller
{
    private readonly IProductService _productService;
    private readonly ShopDbContext _db;

    public HomeController(IProductService productService, ShopDbContext db)
    {
        _productService = productService;
        _db = db;
    }

    public async Task<IActionResult> Index()
    {
        var viewModel = new HomeIndexViewModel
        {
            Banners = await _db.SiteBanners.AsNoTracking().Where(x => x.IsActive).OrderBy(x => x.SortOrder).ToListAsync(),
            MainCategories = await _db.Categories.AsNoTracking().Where(x => x.ParentId == null)
                .OrderBy(x => x.SortOrder).ThenBy(x => x.Name).ToListAsync(),
            DiscountedProducts = await _productService.GetDiscountedProductCardsAsync(),
            NewestProducts = await _productService.GetNewestProductCardsAsync()
        };

        return View(viewModel);
    }
}
