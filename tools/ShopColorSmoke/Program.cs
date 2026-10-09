using System.Text.Json;
using Entities;
using Entities.Enums;
using Microsoft.EntityFrameworkCore;
using Service.Service;

using var settings = JsonDocument.Parse(await File.ReadAllTextAsync("lern/appsettings.json"));
var connection = settings.RootElement.GetProperty("ConnectionStrings").GetProperty("DefaultConnectionstring").GetString();
await using var db = new ShopDbContext(new DbContextOptionsBuilder<ShopDbContext>().UseSqlServer(connection).Options);
await using var transaction = await db.Database.BeginTransactionAsync();
void Check(bool passed, string message) { if (!passed) throw new Exception(message); Console.WriteLine("PASS " + message); }
try
{
    var tag = "color-check-" + Guid.NewGuid().ToString("N");
    var parent = new Category { Name = tag, Slug = tag };
    var child = new Category { Name = tag + "-child", Slug = tag + "-child", Parent = parent };
    var other = new Category { Name = tag + "-other", Slug = tag + "-other" };
    db.Categories.AddRange(parent, child, other);
    Product Add(string suffix, Category category, string colorName, decimal price, bool stock = true, bool active = true)
    {
        var product = new Product { Name = tag + " " + suffix, Slug = tag + "-" + suffix, IsActive = active };
        product.ProductCategories.Add(new ProductCategory { Category = category });
        var option = new ProductSaleOption { Title = "Single", SaleType = EnumSaleType.Single };
        var color = new ProductSaleOptionColor { Color = colorName, HexCode = "#123456" };
        var variant = new ProductVariant { Sku = tag + "-" + suffix, Price = price, StockQuantity = 10, ReservedQuantity = stock ? 0 : 10 };
        option.ProductVariants.Add(variant); color.ProductVariants.Add(variant); option.SaleOptionColors.Add(color);
        product.SaleOptions.Add(option); db.Products.Add(product); return product;
    }
    var blue = Add("blue", child, "آبی", 1000);
    var red = Add("red", parent, "قرمز", 2000);
    Add("pink", other, "صورتی", 3000);
    Add("inactive", parent, "مخفی", 1000, active: false);
    Add("unavailable", child, "سبز", 1000, stock: false);
    Add("blue-again", parent, "آبی", 4000);
    await db.SaveChangesAsync();
    var service = new ProductService(db, null!, null!, null!);
    var all = await service.GetShopProductCardsAsync(tag, [], false, "newest", null, null, 0, 1);
    Check(all.TotalCount == 5 && all.Colors.Count == 4 && all.Colors.Any(c => c.Name == "صورتی"), "all matching colors across all pages, duplicates and inactive products excluded");
    var category = await service.GetShopProductCardsAsync(tag, [parent.Id], false, "newest", null, null, 0, 1);
    Check(category.Colors.Select(c => c.Name).ToHashSet().SetEquals(new[] { "آبی", "قرمز", "سبز" }), "category and descendants exclude unrelated pink");
    var multiple = await service.GetShopProductCardsAsync(tag, [parent.Id], false, "newest", null, null, 0, 10, colors: ["آبی", "قرمز"]);
    Check(multiple.TotalCount == 3 && multiple.SelectedColors.Count == 2 && multiple.Colors.Count == 3, "multiple colors use OR and retain alternatives in facet");
    var priced = await service.GetShopProductCardsAsync(tag, [parent.Id], false, "newest", 1500, 2500, 0, 10);
    Check(priced.TotalCount == 1 && priced.Items.Single().Id == red.Id && priced.Colors.Single().Name == "قرمز", "price filter restricts color options");
    var available = await service.GetShopProductCardsAsync(tag, [parent.Id], true, "newest", null, null, 0, 10);
    Check(available.Colors.Count == 2 && available.Colors.All(c => c.Name != "سبز"), "available filter respects reserved stock per color");
    var stale = await service.GetShopProductCardsAsync(tag, [parent.Id], false, "newest", null, null, 0, 10, colors: ["صورتی"]);
    Check(stale.TotalCount == 4 && stale.SelectedColors.Count == 0, "switching categories removes unavailable color selections");
    var searched = await service.GetShopProductCardsAsync(tag + " pink", [], false, "newest", null, null, 0, 10);
    Check(searched.TotalCount == 1 && searched.Colors.Single().Name == "صورتی", "product search restricts color options");
    var empty = await service.GetShopProductCardsAsync(tag + " missing", [], false, "newest", null, null, 0, 10);
    Check(empty.TotalCount == 0 && empty.Colors.Count == 0, "empty results have empty color options");
}
finally { await transaction.RollbackAsync(); }
