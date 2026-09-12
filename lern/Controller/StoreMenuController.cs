using Entities;
using lern.ViewComponents;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace lern.Controller;

[Route("menu")]
public sealed class StoreMenuController : Microsoft.AspNetCore.Mvc.Controller
{
    private readonly ShopDbContext _db;

    public StoreMenuController(ShopDbContext db) => _db = db;

    [HttpGet("categories")]
    public async Task<IActionResult> Categories()
    {
        var categories = await _db.Categories.AsNoTracking()
            .OrderBy(x => x.SortOrder).ThenBy(x => x.Name).ToListAsync();
        var roots = categories.Where(x => x.ParentId is null).ToList();
        var children = categories.Where(x => x.ParentId is not null)
            .GroupBy(x => x.ParentId!.Value)
            .ToDictionary(x => x.Key, x => (IReadOnlyList<Category>)x.ToList());
        return View("~/Views/Shared/Components/MegaMenu/Default.cshtml", new MegaMenuViewModel(roots, children));
    }
}
