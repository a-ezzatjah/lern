using System.Text.Json;
using System.Xml.Linq;
using AngleSharp.Html.Parser;
using Entities;
using lern.Controller;
using lern.Infrastructure;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Abstractions;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Microsoft.AspNetCore.Routing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;

var root = Path.GetFullPath(args.FirstOrDefault() ?? ".");
void Assert(bool ok, string message) { if (!ok) throw new Exception(message); }
var parser = new HtmlParser();
var rich = "<h1>یک</h1><h2>دو</h2><h3>سه</h3><h4>چهار</h4><h5>پنج</h5><h6>شش</h6><p><strong>بولد</strong> <a href='/product/1'>داخلی</a> <a href='https://example.com' target='_blank' rel='nofollow'>خارجی</a></p>";
var safe = RichText.Render(rich + "<script>alert(1)</script><img src=x onerror=alert(1)><a href='javascript:alert(1)' onclick='alert(2)'>خطر</a><iframe src='https://example.com'></iframe>");
var parsed = parser.ParseDocument(safe);
Assert(parsed.QuerySelectorAll("h1,h2,h3,h4,h5,h6").Length == 6, "Headings lost.");
Assert(parsed.QuerySelector("strong")?.TextContent == "بولد", "Bold lost.");
Assert(parsed.QuerySelector("a[href='/product/1']") is not null, "Internal link lost.");
Assert(parsed.QuerySelector("a[target='_blank']")?.GetAttribute("rel") == "nofollow noopener noreferrer", "External link protections lost.");
Assert(parsed.QuerySelector("script,img,iframe,[onclick]") is null && !safe.Contains("javascript:"), "Unsafe HTML survived.");
Assert(RichText.Render("## عنوان\n\nمتن < غیر HTML").Contains("<h2>عنوان</h2>"), "Legacy heading lost.");
Assert(!RichText.HasText("<p><br></p>") && !RichText.HasText("<script>alert(1)</script>"), "Empty/unsafe article accepted.");
Assert(RichText.Summary("<h2>عنوان</h2><p>توضیح <strong>مهم</strong></p>") == "عنوان توضیح مهم", "Category summary lost readable text.");
Assert(RichText.Summary("<p>" + new string('آ', 300) + "</p>").Length <= 160, "Category meta fallback too long.");
Console.WriteLine("PASS: H1–H6, bold, internal/external links, safe targets, XSS sanitization, legacy content, empty content.");

using var settings = JsonDocument.Parse(await File.ReadAllTextAsync(Path.Combine(root, "lern", "appsettings.json")));
await using var db = new ShopDbContext(new DbContextOptionsBuilder<ShopDbContext>().UseSqlServer(settings.RootElement.GetProperty("ConnectionStrings").GetProperty("DefaultConnectionstring").GetString()).Options);
var configuration = new ConfigurationBuilder().AddInMemoryCollection(new Dictionary<string,string?> { ["Seo:PublicOrigin"] = "https://seo.example" }).Build();
var catalog = new SeoCatalog(db, configuration);
await using (var transaction = await db.Database.BeginTransactionAsync())
{
    var article = new Article { Title = "آزمایش سئو", Slug = "seo-smoke-" + Guid.NewGuid().ToString("N"), Summary = "خلاصه", Content = rich, IsPublished = true };
    db.Articles.Add(article); await db.SaveChangesAsync();
    var controller = new AdminSeoController(db, catalog) { TempData = new TempDataDictionary(new DefaultHttpContext(), new TestTempDataProvider()) };
    var articlePath = "/articles/" + article.Slug;
    Assert(await controller.Save(new SeoPage { Path = articlePath, IndexPage = false, MetaTitle = "عنوان سئو", MetaDescription = "توضیح سئو" }) is RedirectToActionResult, "SEO save failed.");
    var context = new DefaultHttpContext(); context.Request.Path = articlePath;
    var mvc = new Microsoft.AspNetCore.Mvc.ControllerContext(new ActionContext(context, new RouteData(), new Microsoft.AspNetCore.Mvc.Controllers.ControllerActionDescriptor()));
    controller.ControllerContext = mvc;
    var view = new ViewResult { ViewData = controller.ViewData };
    var executing = new ResultExecutingContext(mvc, [], view, controller);
    await new SeoResultFilter(catalog, db).OnResultExecutionAsync(executing, () => Task.FromResult(new ResultExecutedContext(mvc, [], view, controller)));
    Assert(view.ViewData["Robots"]?.ToString() == "noindex, follow", "Noindex not applied to HTML metadata.");
    Assert(context.Response.Headers["X-Robots-Tag"] == "noindex, follow", "Robots HTTP header missing.");
    Assert(view.ViewData["Title"]?.ToString() == "عنوان سئو", "Custom title not applied.");
    Assert(view.ViewData["CanonicalUrl"]?.ToString() == "https://seo.example" + articlePath, "Configured public origin ignored.");
    var sitemap = new SitemapController(catalog) { ControllerContext = mvc };
    var xml = ((ContentResult)await sitemap.Index()).Content!;
    Assert(!xml.Contains(article.Slug), "Noindex article remains in sitemap.");
    await controller.Save(new SeoPage { Path = articlePath });
    Assert(((ContentResult)await sitemap.Index()).Content!.Contains(article.Slug), "Reindexed article missing from sitemap.");
    await controller.Save(new SeoPage { Path = articlePath, IncludeInSitemap = false });
    Assert(((ContentResult)await sitemap.Index()).Content!.Contains(article.Slug) == false, "Explicit sitemap exclusion ignored.");
    var category = await db.Categories.FirstAsync();
    await controller.SaveCategory(category.Id, rich + "<script>bad()</script>", "عنوان دسته", "توضیح دسته");
    Assert(category.Description!.Contains("<strong>") && !category.Description.Contains("<script>"), "Category rich text not safely persisted.");
    var categoryPath = $"/shop?category={category.Id}";
    await controller.Save(new SeoPage { Path = categoryPath, IndexPage = false, MetaTitle = "عنوان دسته", MetaDescription = "توضیح دسته" });
    Assert(category.Seo?.IndexPage == false && (await catalog.Entries()).Single(x => x.Path == categoryPath).IndexPage == false, "Category settings and sitemap diverged.");
    var product = await db.Products.FirstAsync(x => x.IsActive);
    var productPath = $"/product/{product.Id}";
    await controller.Save(new SeoPage { Path = productPath, IndexPage = false });
    Assert(product.Seo?.IndexPage == false && !(await sitemap.Index() as ContentResult)!.Content!.Contains("/product/" + product.Id + "</loc>"), "Product noindex ignored.");
    await controller.Save(new SeoPage { Path = productPath, IncludeInSitemap = false });
    Assert(!(await catalog.Entries()).Single(x => x.Path == productPath).IncludeInSitemap, "Product sitemap exclusion ignored.");
    article.IsPublished = false; await db.SaveChangesAsync();
    Assert(!(await catalog.Entries()).Any(x => x.Path == articlePath), "Draft leaked to sitemap.");
    Assert(await controller.Save(new SeoPage { Path = "/unknown-page" }) is BadRequestResult, "Arbitrary page accepted.");

    // More than two batches exercise pagination and genuine parent/child relationships.
    var suffix = Guid.NewGuid().ToString("N");
    var parent = new Category { Name = "شاخه آزمایشی", Slug = "seo-parent-" + suffix };
    db.Categories.Add(parent); await db.SaveChangesAsync();
    var children = Enumerable.Range(1, 22).Select(i => new Category { Name = "زیرشاخه " + i, Slug = $"seo-child-{suffix}-{i}", ParentId = parent.Id, SortOrder = i }).ToList();
    db.Categories.AddRange(children);
    var group = "مقالات آزمایشی " + suffix;
    db.Articles.AddRange(Enumerable.Range(1, 23).Select(i => new Article { Title = "مقاله " + i, Slug = $"seo-paged-{suffix}-{i}", Category = group, IsPublished = true }));
    await db.SaveChangesAsync();
    var grandchild = new Category { Name = "سطح سوم", Slug = "seo-grandchild-" + suffix, ParentId = children[12].Id };
    db.Categories.Add(grandchild); await db.SaveChangesAsync();
    var tree = await new AdminSeoTree(db, catalog).Build();
    Assert(tree.Select(x => x.Key).SequenceEqual(new[] { "home", "products", "articles", "categories" }), "Sitemap sections missing.");
    var branch = AdminSeoTree.Find(tree, "category:" + parent.Id)!;
    Assert(branch.Children.Count == 22, "Category parent relation lost.");
    var batches = new[] { AdminSeoTree.Children(branch, 0), AdminSeoTree.Children(branch, 10), AdminSeoTree.Children(branch, 20) };
    Assert(batches.Select(x => x.Nodes.Count).SequenceEqual(new[] { 10, 10, 2 }), "Category batch size is not ten.");
    Assert(batches[0].HasMore && batches[1].HasMore && !batches[2].HasMore, "End-of-list flag incorrect.");
    Assert(batches.SelectMany(x => x.Nodes).Select(x => x.Key).Distinct().Count() == 22, "Pagination duplicates or skips categories.");
    Assert(AdminSeoTree.Find(tree, "category:" + children[12].Id)!.Children.Single().Entry?.Path == $"/shop?category={grandchild.Id}", "Deeper hierarchy or preview URL lost.");
    var categoryTree = await new AdminCategorySeoTree(db).Build();
    var editorParent = AdminSeoTree.Find(categoryTree, "category:" + parent.Id)!;
    Assert(editorParent.Kind == "category-editor" && editorParent.Children.Count == 22, "Category editor hierarchy missing.");
    Assert(AdminSeoTree.Find(categoryTree, "category:" + grandchild.Id)!.PreviewPath == $"/shop?category={grandchild.Id}", "Category editor preview missing.");
    var editorPages = new[] { AdminSeoTree.Children(editorParent, 0), AdminSeoTree.Children(editorParent, 10), AdminSeoTree.Children(editorParent, 20) };
    Assert(editorPages.Select(x => x.Nodes.Count).SequenceEqual(new[] { 10, 10, 2 }), "Category editor pagination broken.");
    var editorSearch = AdminSeoTree.Filter(categoryTree, grandchild.Name);
    Assert(AdminSeoTree.Find(editorSearch, "category:" + parent.Id)!.Children.Single().Key == "category:" + children[12].Id, "Category editor search lost ancestors.");
    Assert(await controller.CategoryEditor(-1) is NotFoundResult && await controller.CategoryChildren("unknown") is NotFoundResult &&
        await controller.CategoryChildren("categories", -1) is BadRequestResult && await controller.Categories(new string('x', 101)) is BadRequestResult, "Invalid category editor requests accepted.");
    var editorResult = (PartialViewResult)await controller.CategoryEditor(category.Id);
    Assert(((lern.Models.CategorySeoEditorViewModel)editorResult.Model!).Category.Description == category.Description, "Lazy editor lost content.");
    var editorChildren = (PartialViewResult)await controller.CategoryChildren("category:" + parent.Id, 10);
    Assert(((lern.Models.AdminSeoChildrenViewModel)editorChildren.Model!).Nodes.Count == 10, "Category children endpoint not paginated.");
    var articleBranch = AdminSeoTree.Find(tree, "article-category:" + group)!;
    Assert(articleBranch.Children.Count == 23 && AdminSeoTree.Children(articleBranch, 20).Nodes.Count == 3, "Articles not grouped or paginated correctly.");
    Assert(articleBranch.PreviewPath == "/articles?category=" + Uri.EscapeDataString(group), "Article category preview link invalid.");
    var filtered = AdminSeoTree.Filter(tree, grandchild.Name);
    Assert(filtered.Count == 1 && filtered[0].Key == "categories", "Search retained unrelated sections.");
    Assert(AdminSeoTree.Find(filtered, "category:" + parent.Id)!.Children.Single().Key == "category:" + children[12].Id,
        "Search failed to preserve ancestors of an unloaded grandchild.");
    Assert(AdminSeoTree.Find(filtered, "category:" + grandchild.Id)?.Entry?.Path == $"/shop?category={grandchild.Id}", "Search lost the page link.");
    var articleSearch = AdminSeoTree.Filter(tree, $"seo-paged-{suffix}-");
    Assert(AdminSeoTree.Find(articleSearch, "article-category:" + group)!.Children.Count == 23, "Search missed article URLs.");
    Assert(AdminSeoTree.Children(AdminSeoTree.Find(articleSearch, "article-category:" + group)!, 10).Nodes.Count == 10, "Search pagination broke.");
    Assert(AdminSeoTree.Filter(tree, "no-match-" + suffix).Count == 0, "Empty search returned results.");
    Assert(AdminSeoTree.Filter(tree, "  ").Count == tree.Count, "Clearing search did not restore sections.");
    var spellingNode = new lern.Models.AdminSeoNode { Title = "کالای یک", Kind = "page", Entry = new SeoEntry("/test", "کالای یک") };
    Assert(AdminSeoTree.Filter([spellingNode], "كالاي يك").Count == 1, "Arabic/Persian spelling mismatch.");
    Assert(await controller.Children("article-category:" + group, 10, $"seo-paged-{suffix}-") is PartialViewResult,
        "Search children endpoint failed.");
    Assert(await controller.Children("products", 0, new string('x', 101)) is BadRequestResult, "Oversized query accepted.");
    Assert(await controller.Children("unknown") is NotFoundResult && await controller.Children("products", -1) is BadRequestResult, "Invalid pagination request accepted.");
    var childrenResult = (PartialViewResult)await controller.Children("category:" + parent.Id, 10);
    Assert(((lern.Models.AdminSeoChildrenViewModel)childrenResult.Model!).Nodes.Count == 10, "Children endpoint batch size incorrect.");
    context.Request.Headers["X-Requested-With"] = "XMLHttpRequest";
    var categorySave = (JsonResult)await controller.SaveCategory(category.Id, rich + "<script>bad()</script>", " عنوان دسته ", " توضیح دسته ", true, false, false);
    var categorySavedState = JsonSerializer.SerializeToElement(categorySave.Value);
    Assert(categorySavedState.GetProperty("metaTitle").GetString() == "عنوان دسته" && !categorySavedState.GetProperty("description").GetString()!.Contains("<script>"), "Category inline save did not persist sanitized content.");
    Assert(!categorySavedState.GetProperty("followPage").GetBoolean() && !categorySavedState.GetProperty("isInSitemap").GetBoolean(), "Category indexing preferences ignored.");
    Assert((await catalog.Entries()).Single(x => x.Path == categoryPath).MetaTitle == "عنوان دسته" && !(await catalog.Entries()).Single(x => x.Path == categoryPath).IsInSitemap,
        "Category editor and sitemap settings diverged.");
    Assert(await controller.Categories(grandchild.Name) is PartialViewResult, "Category search did not return a partial.");
    Console.WriteLine("PASS: lightweight category tree, three levels, 10-item batches, full-tree search, lazy editor, sanitized inline save, sitemap settings and invalid requests.");
    async Task<JsonElement> SavedState(SeoPage page)
    {
        var savedResult = await controller.Save(page) as JsonResult;
        Assert(savedResult is not null, "Inline save did not return JSON.");
        return JsonSerializer.SerializeToElement(savedResult!.Value);
    }
    var savedState = await SavedState(new SeoPage { Path = "/", MetaTitle = "  عنوان صفحه اصلی  " });
    Assert(savedState.GetProperty("isInSitemap").GetBoolean() && savedState.GetProperty("metaTitle").GetString() == "عنوان صفحه اصلی", "Saved state not authoritative.");
    savedState = await SavedState(new SeoPage { Path = "/", IndexPage = false });
    Assert(!savedState.GetProperty("isInSitemap").GetBoolean() && !savedState.GetProperty("indexPage").GetBoolean(), "Noindex page badge would disagree with XML.");
    savedState = await SavedState(new SeoPage { Path = "/", IncludeInSitemap = false });
    Assert(!savedState.GetProperty("isInSitemap").GetBoolean() && savedState.GetProperty("indexPage").GetBoolean(), "Excluded indexable page badge would disagree with XML.");
    product.Seo!.CanonicalUrl = "https://seo.example/alternate-product";
    await db.SaveChangesAsync();
    savedState = await SavedState(new SeoPage { Path = productPath });
    Assert(!savedState.GetProperty("isInSitemap").GetBoolean(), "Canonical exclusion missing from saved state.");
    Assert(new SeoEntry("/test", "test").IsInSitemap && !new SeoEntry("/test", "test", IndexPage: false).IsInSitemap &&
        !new SeoEntry("/test", "test", IncludeInSitemap: false).IsInSitemap, "XML inclusion predicate incorrect.");
    Console.WriteLine("PASS: XML inclusion badges reflect index, explicit exclusion and canonical settings; save returns persisted values.");
    Assert(await controller.Index(grandchild.Name) is PartialViewResult, "AJAX search did not return tree partial.");
    Assert(await controller.Index(new string('x', 101)) is BadRequestResult, "Oversized index query accepted.");
    Console.WriteLine("PASS: full-tree title/URL search, preserved ancestors, Persian spelling, empty/reset search, paginated search and query validation.");
    Console.WriteLine("PASS: separate sitemap sections, article category groups, three category levels, 10/10/2 pagination without duplicates, preview links, invalid offsets, inline saving.");
    await transaction.RollbackAsync();
}
Console.WriteLine("PASS: SEO persistence, metadata/HTTP robots, canonical domain, sitemap exclusion/reindexing, category rich text, product consistency, draft exclusion. All test writes rolled back.");

using var client = new HttpClient(new HttpClientHandler { AllowAutoRedirect = false }) { BaseAddress = new Uri("http://localhost:5180") };
var publicXml = XDocument.Parse(await client.GetStringAsync("/sitemap.xml"));
XNamespace ns = "http://www.sitemaps.org/schemas/sitemap/0.9";
Assert(publicXml.Root?.Name == ns + "urlset", "Invalid sitemap schema.");
var urls = publicXml.Descendants(ns + "loc").Select(x => x.Value).ToList();
var articleUrl = urls.First(x => x.Contains("/articles/"));
var response = await client.GetAsync(articleUrl);
var html = parser.ParseDocument(await response.Content.ReadAsStringAsync());
Assert(html.QuerySelector("[itemprop=articleBody] h2") is not null, "Article content not in server-rendered HTML.");
Assert(html.QuerySelectorAll("head title").Length == 1 && html.QuerySelector("link[rel=canonical]") is not null, "Article title/canonical missing or duplicated.");
var productUrl = urls.First(x => x.Contains("/product/"));
html = parser.ParseDocument(await client.GetStringAsync(productUrl));
Assert(html.QuerySelector("[itemprop=description]") is not null, "Product description missing from server HTML.");
html = parser.ParseDocument(await client.GetStringAsync("/shop?q=seo-smoke"));
Assert(html.QuerySelector("meta[name=robots]")?.GetAttribute("content") == "noindex, follow", "Filtered shop should be noindex.");
var publicCategory = await db.Categories.AsNoTracking().OrderByDescending(x => x.Description != null && x.Description != "").FirstAsync();
html = parser.ParseDocument(await client.GetStringAsync($"/shop?category={publicCategory.Id}"));
Assert(html.QuerySelector(".category-seo-description h1")?.TextContent == publicCategory.Name, "Category heading missing from server HTML.");
Assert(html.QuerySelector("[data-category-description]")?.InnerHtml == RichText.Render(publicCategory.Description), "Category description missing or changed in server HTML.");
var expectedCategoryTitle = string.IsNullOrWhiteSpace(publicCategory.Seo?.MetaTitle) ? publicCategory.Name : publicCategory.Seo.MetaTitle;
Assert(html.QuerySelector("title")?.TextContent.StartsWith(expectedCategoryTitle) == true, "Category SEO title missing from server HTML.");
var expectedCategoryMeta = string.IsNullOrWhiteSpace(publicCategory.Seo?.MetaDescription) ? RichText.Summary(publicCategory.Description) : publicCategory.Seo.MetaDescription;
if (!string.IsNullOrWhiteSpace(expectedCategoryMeta)) Assert(html.QuerySelector("meta[name=description]")?.GetAttribute("content") == expectedCategoryMeta, "Category meta description missing from server HTML.");
Assert(html.QuerySelector("link[rel=canonical]") is not null, "Category canonical missing.");
Console.WriteLine("PASS: public category heading, sanitized description, SEO title/meta fallback and canonical in server-rendered HTML.");
Assert((await client.GetStringAsync("/robots.txt")).Contains("Sitemap: http://localhost:5180/sitemap.xml"), "Robots sitemap missing.");
foreach (var url in new[] { "/Admin/Seo", "/Admin/Seo/children?key=categories", "/Admin/Settings/category-seo", "/Admin/Settings/category-seo/children?key=categories", $"/Admin/Settings/category-seo/{publicCategory.Id}/editor" }) Assert((int)(await client.GetAsync(url)).StatusCode == 302, "Unprotected SEO admin route.");
Console.WriteLine("PASS: live sitemap XML, article/product server HTML, canonical/title, filtered noindex, robots.txt, admin authorization.");

sealed class TestTempDataProvider : ITempDataProvider
{
    public IDictionary<string, object> LoadTempData(HttpContext context) => new Dictionary<string, object>();
    public void SaveTempData(HttpContext context, IDictionary<string, object> values) { }
}
