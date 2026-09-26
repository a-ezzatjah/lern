using System.Text.Json;
using Entities;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Service.Service;
using lern.Controller;

// Read-only integration check. Does not start the app, seed data, or run migrations.
var settingsPath = args.FirstOrDefault() ?? "lern/appsettings.json";
using var settings = JsonDocument.Parse(await File.ReadAllTextAsync(settingsPath));
var connection = settings.RootElement.GetProperty("ConnectionStrings").GetProperty("DefaultConnectionstring").GetString();
await using var db = new ShopDbContext(new DbContextOptionsBuilder<ShopDbContext>().UseSqlServer(connection).Options);
var service = new ProductService(db, null!, null!, null!);
var categories = await db.Categories.AsNoTracking().ToListAsync();
var products = await db.Products.AsNoTracking().Where(p => p.IsActive)
    .Select(p => new { p.Id, p.CreatedAt, CategoryIds = p.ProductCategories.Select(c => c.CategoryId).ToList() }).ToListAsync();
if (categories.Count == 0) throw new Exception("No categories available for an integration check.");
var byId = categories.ToDictionary(c => c.Id);
bool IsDescendant(int id, int ancestor)
{
    var visited = new HashSet<int>();
    while (visited.Add(id))
    {
        if (id == ancestor) return true;
        if (!byId.TryGetValue(id, out var category) || !category.ParentId.HasValue) return false;
        id = category.ParentId.Value;
    }
    return false;
}
var selected = categories.Where(c => c.ParentId == null).Take(2)
    .Concat(categories.Where(c => !categories.Any(other => other.ParentId == c.Id)).Take(2))
    .Concat(categories.Where(c => products.Any(p => p.CategoryIds.Contains(c.Id))).Take(2))
    .Concat(categories.Where(c => categories.Any(child => child.ParentId == c.Id)
        && products.Any(p => p.CategoryIds.Any(id => IsDescendant(id, c.Id)))).Take(2))
    .DistinctBy(c => c.Id).ToList();
foreach (var category in selected)
{
    var expected = products.Where(p => p.CategoryIds.Any(id => IsDescendant(id, category.Id)))
        .OrderByDescending(p => p.CreatedAt).ThenByDescending(p => p.Id).Select(p => p.Id).ToList();
    var first = await service.GetCategoryProductCardsAsync(category.Id, -1);
    if (first.TotalCount != expected.Count || first.Page != 1 || !first.Items.Select(p => p.Id).SequenceEqual(expected.Take(24)))
        throw new Exception($"First-page category filtering failed: {category.Id}");
    var last = await service.GetCategoryProductCardsAsync(category.Id, int.MaxValue);
    var lastPage = Math.Max(1, (int)Math.Ceiling(expected.Count / 24d));
    if (last.Page != lastPage || !last.Items.Select(p => p.Id).SequenceEqual(expected.Skip((lastPage - 1) * 24)))
        throw new Exception($"Last-page category filtering failed: {category.Id}");
    var result = await new StoreCategoryController(db, service).Index(category.Id);
    if (result is not ViewResult { Model: lern.Models.StoreCategoryViewModel model } || model.Category.Id != category.Id)
        throw new Exception("Category controller failed.");
    Console.WriteLine($"PASS category {category.Id}: {expected.Count} active products, first/last page, controller.");
}
if (await new StoreCategoryController(db, service).Index(-1) is not NotFoundResult)
    throw new Exception("Unknown category must return 404.");
Console.WriteLine("PASS unknown category returns 404. All checks were read-only.");
