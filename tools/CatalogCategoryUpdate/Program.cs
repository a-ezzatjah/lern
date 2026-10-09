using System.Text.Json;
using Entities;
using Microsoft.EntityFrameworkCore;
using var settings = JsonDocument.Parse(await File.ReadAllTextAsync("lern/appsettings.json"));
var connection = settings.RootElement.GetProperty("ConnectionStrings").GetProperty("DefaultConnectionstring").GetString();
await using var db = new ShopDbContext(new DbContextOptionsBuilder<ShopDbContext>().UseSqlServer(connection).Options);
var pending = (await db.Database.GetPendingMigrationsAsync()).ToArray();
Console.WriteLine("Pending migrations: " + string.Join(",", pending));
void Check(bool condition, string message)
{
    if (!condition) throw new Exception(message);
    Console.WriteLine("PASS " + message);
}
async Task VerifyAsync()
{
    var categories = await db.Categories.AsNoTracking().ToListAsync();
    var elastic = categories.Single(c => c.Name == "کش");
    var iranian = categories.Single(c => c.Name == "کش ایرانی");
    Check(elastic.ParentId == null && new[] { "کش قیطان", "کش ایرانی", "کش خارجی" }
        .All(name => categories.Any(c => c.Name == name && c.ParentId == elastic.Id)), "elastic hierarchy");
    Check(categories.All(c => c.Slug != "buttons-and-hardware"), "unused legacy category removed");
    var products = await db.Products.AsNoTracking().Where(p => p.Name == "کش سه سانت" || p.Name == "کش 1 سانت")
        .Select(p => new { p.Id, Categories = p.ProductCategories.Select(pc => pc.CategoryId).ToList() }).ToListAsync();
    Check(products.Count == 2 && products.All(p => p.Categories.SequenceEqual(new[] { iranian.Id })), "both elastic products assigned to Iranian elastic");
    var trail = lern.Models.StoreProductDetailsViewModel.BuildCategoryTrail([iranian.Id, elastic.Id], categories);
    Check(trail.Select(c => c.Name).SequenceEqual(new[] { "کش", "کش ایرانی" }) && trail.Last().Url == $"/shop?category={iranian.Id}", "dynamic product trail uses deepest actual category with working links");
    Check(lern.Models.StoreProductDetailsViewModel.BuildCategoryTrail([], categories).Count == 0, "uncategorized product has no fabricated category");
    var cyclic = new[] { new Category { Id = -1, Name = "A", Slug = "a", ParentId = -2 }, new Category { Id = -2, Name = "B", Slug = "b", ParentId = -1 } };
    Check(lern.Models.StoreProductDetailsViewModel.BuildCategoryTrail([-1], cyclic).Count == 2, "invalid category cycle terminates safely");
    var service = new Service.Service.ProductService(db, null!, null!, null!);
    var page = await service.GetShopProductCardsAsync(null, [elastic.Id], false, "newest", null, null, 0, 10);
    Check(products.All(p => page.Items.Any(card => card.Id == p.Id)), "root elastic filter includes Iranian elastic products");
}
if (args.Contains("--verify"))
{
    await using var transaction = await db.Database.BeginTransactionAsync();
    if (pending.Contains("20261009120000_AddElasticCategories"))
        foreach (var operation in new Entities.Migrations.AddElasticCategories().UpOperations.Cast<Microsoft.EntityFrameworkCore.Migrations.Operations.SqlOperation>())
            await db.Database.ExecuteSqlRawAsync(operation.Sql);
    foreach (var operation in new Entities.Migrations.CorrectElasticCordCategoryName().UpOperations.Cast<Microsoft.EntityFrameworkCore.Migrations.Operations.SqlOperation>())
        await db.Database.ExecuteSqlRawAsync(operation.Sql);
    await VerifyAsync();
    // A second run must not duplicate categories or associations.
    foreach (var operation in new Entities.Migrations.CorrectElasticCordCategoryName().UpOperations.Cast<Microsoft.EntityFrameworkCore.Migrations.Operations.SqlOperation>())
        await db.Database.ExecuteSqlRawAsync(operation.Sql);
    await VerifyAsync();
    await transaction.RollbackAsync();
    Console.WriteLine("Dry run rolled back; migration is idempotent.");
}
if (args.Contains("--apply"))
{
    Check(pending.All(id => id is "20261009120000_AddElasticCategories" or "20261009123000_CorrectElasticCordCategoryName"), "only requested catalog migration is pending");
    await db.Database.MigrateAsync();
    await VerifyAsync();
    Console.WriteLine("Applied catalog migration.");
}
