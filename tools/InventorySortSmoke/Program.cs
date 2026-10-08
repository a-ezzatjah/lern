using System.Text.Json;
using Entities;
using Entities.Enums;
using Microsoft.EntityFrameworkCore;
using Service.Service;
using ServiceContract.DTO.DtoProduct;

// Every fixture is rolled back. No application startup or migrations are run.
using var settings = JsonDocument.Parse(await File.ReadAllTextAsync("lern/appsettings.json"));
var connection = settings.RootElement.GetProperty("ConnectionStrings").GetProperty("DefaultConnectionstring").GetString();
await using var db = new ShopDbContext(new DbContextOptionsBuilder<ShopDbContext>().UseSqlServer(connection).Options);
await using var transaction = await db.Database.BeginTransactionAsync();
void Check(bool passed, string message) { if (!passed) throw new Exception(message); Console.WriteLine("PASS " + message); }
try
{
    var tag = "stock-check-" + Guid.NewGuid().ToString("N");
    var parent = new Category { Name = tag, Slug = tag };
    var child = new Category { Name = tag + "-child", Slug = tag + "-child", Parent = parent };
    db.Categories.AddRange(parent, child);
    var fixtures = new List<Product>();
    var now = DateTime.UtcNow;
    for (var i = 0; i < 32; i++)
    {
        var product = new Product { Name = tag + "-" + i, Slug = tag + "-" + i, CreatedAt = now.AddMinutes(i), IsActive = i < 31 };
        product.ProductCategories.Add(new ProductCategory { Category = i % 2 == 0 ? parent : child });
        if (i != 30)
        {
            var option = new ProductSaleOption { Title = "Single", SaleType = EnumSaleType.Single };
            var variant = new ProductVariant { Sku = tag + "-sku-" + i, Price = i == 0 ? 0 : 100 + i, StockQuantity = i < 13 ? 10 : 5, ReservedQuantity = i < 13 ? 2 : 5 };
            option.ProductVariants.Add(variant);
            if (i == 12)
            {
                var color = new ProductSaleOptionColor { Color = "Blue" };
                color.ProductVariants.Add(variant); option.SaleOptionColors.Add(color);
            }
            product.SaleOptions.Add(option);
        }
        db.Products.Add(product); fixtures.Add(product);
    }
    await db.SaveChangesAsync();
    var service = new ProductService(db, null!, null!, null!);
    foreach (var categoryIds in new[] { Array.Empty<int>(), new[] { parent.Id } })
    foreach (var sort in new[] { "newest", "popular", "bestselling", "price_asc", "price_desc" })
    {
        var cards = new List<ProductCardDto>();
        for (var offset = 0; offset < 31; offset += 6)
        {
            var batch = await service.GetShopProductCardsAsync(tag, categoryIds, false, sort, null, null, offset, 6);
            Check(batch.TotalCount == 31, $"{sort} total count at offset {offset}");
            cards.AddRange(batch.Items);
        }
        Check(cards.Count == 31 && cards.Select(c => c.Id).Distinct().Count() == 31 && cards.Take(13).All(c => c.IsAvailable) && cards.Skip(13).All(c => !c.IsAvailable), $"{sort}, categories={categoryIds.Length}: all 13 available before 18 unavailable across scroll batches");
        if (sort == "newest")
        {
            var expected = fixtures.Where(p => p.IsActive).OrderByDescending(p => fixtures.IndexOf(p) < 13).ThenByDescending(p => p.CreatedAt).ThenByDescending(p => p.Id).Select(p => p.Id);
            Check(cards.Select(c => c.Id).SequenceEqual(expected), "newest sorting preserved within availability groups");
        }
        if (sort is "price_asc" or "price_desc")
        {
            var expected = fixtures.Skip(1).Take(12).OrderBy(p => sort == "price_asc" ? p.SaleOptions.First().ProductVariants.First().Price : -p.SaleOptions.First().ProductVariants.First().Price).Select(p => p.Id).Append(fixtures[0].Id);
            Check(cards.Take(13).Select(c => c.Id).SequenceEqual(expected), "price ordering preserved, available zero-price product stays before unavailable products");
        }
    }
    var onlyAvailable = await service.GetShopProductCardsAsync(tag, [parent.Id], true, "newest", null, null, 0, 10);
    Check(onlyAvailable.TotalCount == 13 && onlyAvailable.Items.All(c => c.IsAvailable), "available-only filter respects reserved stock and excludes inactive products");
    var first = await service.GetCategoryProductCardsAsync(parent.Id, 1);
    var last = await service.GetCategoryProductCardsAsync(parent.Id, int.MaxValue);
    var categoryCards = first.Items.Concat(last.Items).ToList();
    Check(first.TotalCount == 31 && first.Items.Count == 24 && last.Page == 2 && last.Items.Count == 7 && categoryCards.Take(13).All(c => c.IsAvailable) && categoryCards.Skip(13).All(c => !c.IsAvailable), "category and descendants: available first across page boundary");
    Console.WriteLine("PASS inventory sorting checks; fixtures will be rolled back.");
}
finally { await transaction.RollbackAsync(); }
