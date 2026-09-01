using Microsoft.AspNetCore.Mvc;
using lern.Models;
using ServiceContract.Interfaces;

namespace lern.Controller;

[Route("product")]
public class StoreProductController : Microsoft.AspNetCore.Mvc.Controller
{
    private readonly IProductService _products;
    public StoreProductController(IProductService products) => _products = products;

    [HttpGet("{id:int}")]
    public async Task<IActionResult> Details(int id)
    {
        var product = await _products.GetPageAsync(id);
        if (product is null)
            return NotFound();

        return View(StoreProductDetailsViewModel.FromProduct(product));
    }
}
