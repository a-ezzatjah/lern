using System.Text.Json;
using System.Xml.Linq;
using AngleSharp.Html.Parser;
using Entities;
using lern.Controller;
using lern.Infrastructure;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Routing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;

var root = Path.GetFullPath(args.FirstOrDefault() ?? ".");
using var settings = JsonDocument.Parse(await File.ReadAllTextAsync(Path.Combine(root, "lern", "appsettings.json")));
await using var db = new ShopDbContext(new DbContextOptionsBuilder<ShopDbContext>().UseSqlServer(settings.RootElement.GetProperty("ConnectionStrings").GetProperty("DefaultConnectionstring").GetString()).Options);
var category = await db.Categories.AsNoTracking().SingleAsync(x => x.Id == 2 && x.Name == "نخ");
using var sample = JsonDocument.Parse(await File.ReadAllTextAsync(Path.Combine(root, "scripts", "category-seo-sample.json")));
var content = sample.RootElement;
var mode = args.Skip(1).FirstOrDefault() ?? "--inspect";
void Assert(bool condition, string message) { if (!condition) throw new Exception(message); }
var path = $"/shop?category={category.Id}";
var parser = new HtmlParser();
var title = content.GetProperty("metaTitle").GetString()!;
var meta = content.GetProperty("metaDescription").GetString()!;
var description = RichText.Render(content.GetProperty("description").GetString());
Assert(content.GetProperty("categoryId").GetInt32() == category.Id && content.GetProperty("categoryName").GetString() == category.Name, "Sample target does not match category.");
if (mode == "--apply")
{
    Assert(string.IsNullOrWhiteSpace(category.Description) && string.IsNullOrWhiteSpace(category.Seo?.MetaTitle) && string.IsNullOrWhiteSpace(category.Seo?.MetaDescription), "Category already has content. Refusing to replace it.");
    Assert(category.Seo?.IndexPage != false && category.Seo?.FollowPage != false && string.IsNullOrWhiteSpace(category.Seo?.CanonicalUrl), "Category has existing SEO restrictions. Refusing to replace them.");
    var oldPage = await db.SeoPages.AsNoTracking().SingleOrDefaultAsync(x => x.Path == path);
    Assert(oldPage is null, "Category has preexisting sitemap settings. Refusing to replace them.");
    Directory.CreateDirectory(Path.Combine(root, "tmp"));
    await File.WriteAllTextAsync(Path.Combine(root, "tmp", "category-seo-sample-backup.json"), JsonSerializer.Serialize(new { category.Id, category.Name, category.Description, category.Seo, Sitemap = oldPage }));
    await using var transaction = await db.Database.BeginTransactionAsync();
    var configuration = new ConfigurationBuilder().AddInMemoryCollection().Build();
    var catalog = new SeoCatalog(db, configuration);
    var http = new DefaultHttpContext();
    http.Request.Headers["X-Requested-With"] = "XMLHttpRequest";
    var mvc = new ControllerContext(new ActionContext(http, new RouteData(), new Microsoft.AspNetCore.Mvc.Controllers.ControllerActionDescriptor()));
    var controller = new AdminSeoController(db, catalog) { ControllerContext = mvc };
    var result = await controller.SaveCategory(category.Id, description, title, meta, true, true, true) as JsonResult;
    Assert(result is not null && JsonSerializer.SerializeToElement(result.Value).GetProperty("isInSitemap").GetBoolean(), "Sample save failed.");
    await transaction.CommitAsync();
    Console.WriteLine("SAVED: sample SEO for category 2 only. Original values backed up to tmp/category-seo-sample-backup.json.");
}
if (mode is "--apply" or "--verify" or "--verify-accordion")
{
    var saved = await db.Categories.AsNoTracking().SingleAsync(x => x.Id == category.Id);
    Assert(saved.Description == description && saved.Seo?.MetaTitle == title && saved.Seo.MetaDescription == meta, "Saved content differs from sample.");
    using var client = new HttpClient { BaseAddress = new Uri("http://localhost:5180") };
    var response = await client.GetAsync(path);
    Assert(response.IsSuccessStatusCode, "Category page does not return success.");
    var html = parser.ParseDocument(await response.Content.ReadAsStringAsync());
    Assert(html.QuerySelectorAll("h1").Length == 1 && html.QuerySelector(".category-seo-description h1")?.TextContent == category.Name, "Category heading missing or duplicated.");
    Assert(html.QuerySelector("title")?.TextContent == title + " - Kohestani", "SEO title missing from server HTML.");
    Assert(html.QuerySelector("meta[name=description]")?.GetAttribute("content") == meta, "Meta description missing.");
    Assert(html.QuerySelector("meta[name=robots]")?.GetAttribute("content") == "index, follow", "Category indexing is disabled.");
    Assert(html.QuerySelector("link[rel=canonical]")?.GetAttribute("href") == "http://localhost:5180" + path, "Canonical differs from category URL.");
    var body = html.QuerySelector("[data-category-description]")!;
    var accordion = html.QuerySelector("details.category-description-accordion");
    Assert(accordion is not null && !accordion.HasAttribute("open"), "Description accordion should start closed.");
    Assert(accordion!.QuerySelector("summary .category-description-preview")?.TextContent.Length > 0 &&
        accordion.QuerySelector(".category-description-more")?.TextContent == "مشاهده بیشتر", "Collapsed preview or expand control missing.");
    Assert(body is not null && body.QuerySelectorAll("h2").Length == 2 && body.QuerySelector("h3") is not null && body.QuerySelectorAll("strong").Length >= 2, "Formatted description missing from HTML.");
    Assert(body!.InnerHtml == description && body.QuerySelector("script,[onclick],img") is null, "Description unsafe or not server-rendered.");
    if (mode == "--verify-accordion")
    {
        Console.WriteLine("PASS: accordion initially closed, preview and show-more control, full description still in server HTML, title/meta, index/follow and canonical.");
        return;
    }
    Assert(body.QuerySelectorAll("a").Length == 5, "Sample internal links missing.");
    foreach (var link in body.QuerySelectorAll("a"))
    {
        var target = link.GetAttribute("href")!;
        Assert((await client.GetAsync(target)).IsSuccessStatusCode, "Broken internal link: " + target);
    }
    var sitemap = XDocument.Parse(await client.GetStringAsync("/sitemap.xml"));
    XNamespace ns = "http://www.sitemaps.org/schemas/sitemap/0.9";
    Assert(sitemap.Descendants(ns + "loc").Any(x => x.Value == "http://localhost:5180" + path), "Sample missing from sitemap.");
    Console.WriteLine("PASS: saved values, HTTP 200, single H1, H2/H3/bold, server-rendered description, title/meta, index/follow, canonical, five internal links and sitemap inclusion.");
    Console.WriteLine("PREVIEW: http://localhost:5180" + path);
    return;
}
Assert(mode == "--inspect", "Use --inspect, --apply or --verify.");
Console.WriteLine(JsonSerializer.Serialize(new { category.Id, category.Name, category.Description, category.Seo,
    Children = await db.Categories.AsNoTracking().Where(x => x.ParentId == category.Id).OrderBy(x => x.SortOrder).ThenBy(x => x.Id).Select(x => new { x.Id, x.Name }).ToListAsync(),
    Sitemap = await db.SeoPages.AsNoTracking().SingleOrDefaultAsync(x => x.Path == "/shop?category=2") }, new JsonSerializerOptions { Encoder = System.Text.Encodings.Web.JavaScriptEncoder.UnsafeRelaxedJsonEscaping }));
