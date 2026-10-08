using System.Text.Json;
using Entities;
using Microsoft.EntityFrameworkCore;
using Service.Search;
using Service.Service;
using lern.Controller;
using Microsoft.AspNetCore.Mvc;

void Check(bool condition, string message)
{
    if (!condition) throw new Exception(message);
    Console.WriteLine("PASS " + message);
}

var samples = new[]
{
    new Product { Id = 1, Name = "کش سه سانت" },
    new Product { Id = 2, Name = "کش دو سانت" },
    new Product { Id = 3, Name = "روبان یک سانت" },
    new Product { Id = 4, Name = "سوزن" },
    new Product { Id = 5, Name = "كش ۳ سانت" },
    new Product { Id = 6, Name = "کش ٣ سانت" },
    new Product { Id = 7, Name = "نخ خياطی" },
    new Product { Id = 8, Name = "منگنه 23" }
}.AsQueryable();
Check(ProductNameSearch.Apply(samples, "کش 3 سانت").Select(p => p.Id).SequenceEqual(new[] { 1, 2, 5, 6 }),
    "all textual words required: different elastic sizes match, ribbon and staple 23 excluded");
Check(ProductNameSearch.Apply(samples, "کش").Count() == 4, "single word still matches products");
foreach (var digit in new[] { "3", "۳", "٣" })
    Check(ProductNameSearch.Apply(samples, digit).Select(p => p.Id).SequenceEqual(new[] { 5, 6, 8 }),
        "standalone digit " + digit + " matches staple 23 and normalized numbers");
Check(ProductNameSearch.Apply(samples, "ناموجود کش").Count() == 0, "an additional word narrows results");
Check(ProductNameSearch.Apply(samples, "سانت کش").Select(p => p.Id).SequenceEqual(new[] { 1, 2, 5, 6 }),
    "word order does not affect matching");
Check(ProductNameSearch.Apply(samples, "کش ۳ سانت").Select(p => p.Id).SequenceEqual(new[] { 1, 2, 5, 6 }) &&
    ProductNameSearch.Apply(samples, "کش ٣ سانت").Select(p => p.Id).SequenceEqual(new[] { 1, 2, 5, 6 }),
    "Persian, Arabic and English digits preserve named-product results");
Check(ProductNameSearch.Apply(samples, "خیاطی").Single().Id == 7, "Arabic and Persian letter variants match");
Check(ProductNameSearch.Apply(samples, "  کش\tکش\u200cسانت  ").Count() == 4, "whitespace, half spaces and duplicate words");
Check(ProductNameSearch.Apply(samples, "ناموجود").Count() == 0, "no matching word returns no products");
Check(ProductNameSearch.Apply(samples, "\u200c  ").Count() == 8, "empty normalized query preserves the catalogue");

// Read-only integration checks against the configured SQL Server; no fixtures or migrations.
using var settings = JsonDocument.Parse(await File.ReadAllTextAsync("lern/appsettings.json"));
var connection = settings.RootElement.GetProperty("ConnectionStrings").GetProperty("DefaultConnectionstring").GetString();
await using var db = new ShopDbContext(new DbContextOptionsBuilder<ShopDbContext>().UseSqlServer(connection).Options);
var active = db.Products.AsNoTracking().Where(p => p.IsActive);
var expected = await active.Where(p => (p.Name.Contains("کش") || p.Name.Contains("كش")) && p.Name.Contains("سانت"))
    .Select(p => p.Id).ToListAsync();
var matches = await ProductNameSearch.Apply(active, "کش 3 سانت").Select(p => p.Id).ToListAsync();
Check(expected.Count > 0 && expected.ToHashSet().SetEquals(matches), "SQL translation requires both product words");
var service = new ProductService(db, null!, null!, null!);
var result = await service.GetShopProductCardsAsync("کش 3 سانت", [], false, "newest", null, null, 0, 10);
Check(result.TotalCount == expected.Count && result.Items.All(p => expected.Contains(p.Id)),
    "storefront service preserves matching count and pagination");
var persian = await service.GetShopProductCardsAsync("کش ۳ سانت", [], false, "newest", null, null, 0, 10);
Check(result.TotalCount == persian.TotalCount && result.Items.Select(p => p.Id).SequenceEqual(persian.Items.Select(p => p.Id)),
    "Persian and English query digits return the same ordered storefront results");
var controller = new ShopController(db, service);
foreach (var digit in new[] { "1", "۱", "١", "3", "۳", "٣" })
{
    var expectedDigits = await service.GetShopProductCardsAsync(digit, [], false, "newest", null, null, 0, 10);
    var suggestion = (OkObjectResult)await controller.Suggest(digit);
    var data = JsonSerializer.SerializeToElement(suggestion.Value);
    var ids = data.GetProperty("items").EnumerateArray().Select(item => item.GetProperty("Id").GetInt32());
    Check(ids.SequenceEqual(expectedDigits.Items.Select(item => item.Id)),
        "suggest endpoint accepts standalone digit " + digit);
}
