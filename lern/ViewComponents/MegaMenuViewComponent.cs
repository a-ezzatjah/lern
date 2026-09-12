using Entities;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace lern.ViewComponents;

public sealed class MegaMenuViewComponent : ViewComponent
{
    private readonly ShopDbContext _db;

    public MegaMenuViewComponent(ShopDbContext db) => _db = db;

    public async Task<IViewComponentResult> InvokeAsync()
    {
        var categories = await _db.Categories.AsNoTracking()
            .OrderBy(x => x.SortOrder).ThenBy(x => x.Name)
            .ToListAsync();

        var roots = categories.Where(x => x.ParentId is null).ToList();
        var childrenByParent = categories.Where(x => x.ParentId is not null)
            .GroupBy(x => x.ParentId!.Value)
            .ToDictionary(x => x.Key, x => (IReadOnlyList<Category>)x.ToList());

        return View(new MegaMenuViewModel(roots, childrenByParent));
    }
}

public sealed record MegaMenuViewModel(
    IReadOnlyList<Category> Roots,
    IReadOnlyDictionary<int, IReadOnlyList<Category>> ChildrenByParent);
