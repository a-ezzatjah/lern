using Entities;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace lern.Controller;

[Route("category")]
public sealed class StoreCategoryController(ShopDbContext db) : Microsoft.AspNetCore.Mvc.Controller
{
    [HttpGet("{id:int}")]
    public async Task<IActionResult> Index(int id, int page = 1)
    {
        if (!await db.Categories.AsNoTracking().AnyAsync(c => c.Id == id)) return NotFound();
        return RedirectToAction("Index", "Shop", new { category = id, page });
    }
}
