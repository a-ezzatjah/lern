using Microsoft.AspNetCore.Mvc;
using lern.Models;
using ServiceContract.Interfaces;

namespace lern.Controller;

public class HomeController : Microsoft.AspNetCore.Mvc.Controller
{
    private readonly IProductService _productService;

    public HomeController(IProductService productService)
    {
        _productService = productService;
    }

    public async Task<IActionResult> Index()
    {
        var viewModel = new HomeIndexViewModel
        {
            DiscountedProducts =
                await _productService.GetDiscountedProductCardsAsync()
        };

        return View(viewModel);
    }
}
