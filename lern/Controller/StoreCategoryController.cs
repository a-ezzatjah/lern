using Entities;
using lern.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ServiceContract.Interfaces;

namespace lern.Controller;

[Route("category")]
public sealed class StoreCategoryController(ShopDbContext db, IProductService products) : Microsoft.AspNetCore.Mvc.Controller
{
    [HttpGet("{id:int}")]
    public async Task<IActionResult> Index(int id, int page = 1)
    {
        var categories = await db.Categories.AsNoTracking()
            .OrderBy(c => c.SortOrder).ThenBy(c => c.Name).ToListAsync();
        var byId = categories.ToDictionary(c => c.Id);
        if (!byId.TryGetValue(id, out var category)) return NotFound();

        var breadcrumbs = new List<Category>();
        var visited = new HashSet<int> { id };
        var parentId = category.ParentId;
        while (parentId.HasValue && visited.Add(parentId.Value) && byId.TryGetValue(parentId.Value, out var parent))
        {
            breadcrumbs.Add(parent);
            parentId = parent.ParentId;
        }
        breadcrumbs.Reverse();
        var result = await products.GetCategoryProductCardsAsync(id, page);
        ViewData["Title"] = category.Name;
        return View(new StoreCategoryViewModel(category, breadcrumbs,
            categories.Where(c => c.ParentId == id).ToList(), result));
    }
}
