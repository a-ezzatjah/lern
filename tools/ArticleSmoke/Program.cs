using System.Text.Json;
using System.ComponentModel.DataAnnotations;
using Entities;
using lern.Controller;
using lern.Infrastructure;
using lern.Models;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.FileProviders;

// Database writes are rolled back; uploaded test files stay in tmp, outside the live web root.
var root = Path.GetFullPath(args.FirstOrDefault() ?? ".");
using var settings = JsonDocument.Parse(await File.ReadAllTextAsync(Path.Combine(root, "lern", "appsettings.json")));
var connection = settings.RootElement.GetProperty("ConnectionStrings").GetProperty("DefaultConnectionstring").GetString();
await using var db = new ShopDbContext(new DbContextOptionsBuilder<ShopDbContext>().UseSqlServer(connection).Options);
var environment = new TestEnvironment(Path.Combine(root, "lern"), Path.Combine(root, "tmp", "article-smoke", Guid.NewGuid().ToString("N")));
Directory.CreateDirectory(environment.WebRootPath);
await using var transaction = await db.Database.BeginTransactionAsync();
void Assert(bool passed, string message) { if (!passed) throw new Exception(message); }
AdminArticlesController Admin() => new(db, environment)
{
    TempData = new TempDataDictionary(new DefaultHttpContext(), new TestTempDataProvider())
};
try
{
    var before = await db.Articles.CountAsync();
    await ArticleSeeder.SeedAsync(db, environment);
    Assert(await db.Articles.CountAsync() == before, "Seeder changed existing articles.");
    var slug = "smoke-" + Guid.NewGuid().ToString("N");
    var bytes = await File.ReadAllBytesAsync(Path.Combine(root, "lern", "wwwroot", "assets", "images", "articles", "sewing-essentials.jpg"));
    using var imageStream = new MemoryStream(bytes);
    var form = new ArticleForm { Title = "مقاله آزمایشی", Slug = slug, Category = "آزمایش", Author = "آزمایش",
        Summary = "خلاصه آزمایشی", Content = "## عنوان بخش\n\nمتن آزمایشی <script>alert(1)</script>", ImageAlt = "تصویر آزمایشی",
        Image = new FormFile(imageStream, 0, bytes.Length, "Image", "test.jpg") };
    Assert(Validator.TryValidateObject(form, new ValidationContext(form), [], true), "Valid article form rejected.");
    Assert(await Admin().Save(null, form) is RedirectToActionResult, "Create failed.");
    var article = await db.Articles.SingleAsync(x => x.Slug == slug);
    Assert(!article.IsPublished && article.PublishedAt is null, "New draft was published.");
    Assert(File.Exists(Path.Combine(environment.WebRootPath, article.ImageUrl.TrimStart('/'))), "Image was not saved.");
    var publicController = new ArticlesController(db);
    Assert(await publicController.Details(slug) is NotFoundResult, "Draft exposed publicly.");
    var draftList = (ViewResult)await publicController.Index(slug, null);
    Assert(((ArticleListViewModel)draftList.Model!).Articles.Count == 0, "Draft included in public search.");
    form.Image = null; form.IsPublished = true;
    Assert(await Admin().Save(article.Id, form) is RedirectToActionResult, "Publish failed.");
    Assert(article.PublishedAt.HasValue && await publicController.Details(slug) is ViewResult, "Published article unavailable.");
    var categoryList = (ArticleListViewModel)((ViewResult)await publicController.Index(null, "آزمایش", int.MaxValue)).Model!;
    Assert(categoryList.Articles.Count == 1 && categoryList.Page == 1, "Category or page clamping failed.");
    var originalPublishedAt = article.PublishedAt;
    form.Title = "عنوان ویرایش‌شده";
    Assert(await Admin().Save(article.Id, form) is RedirectToActionResult && article.Title == form.Title && article.PublishedAt == originalPublishedAt, "Edit failed.");
    var duplicate = Admin();
    Assert(await duplicate.Save(null, form) is ViewResult && !duplicate.ModelState.IsValid, "Duplicate slug or missing image accepted.");
    var invalid = Admin();
    using var badStream = new MemoryStream(new byte[64]);
    form.Slug = "bad-image-" + Guid.NewGuid().ToString("N");
    form.Image = new FormFile(badStream, 0, 64, "Image", "bad.jpg");
    Assert(await invalid.Save(null, form) is ViewResult && invalid.ModelState.ContainsKey(nameof(form.Image)), "Invalid image accepted.");
    form.Image = null; form.Slug = slug; form.IsPublished = false;
    Assert(await Admin().Save(article.Id, form) is RedirectToActionResult && await publicController.Details(slug) is NotFoundResult, "Unpublished article remains public.");
    Console.WriteLine("PASS: seed idempotence, image upload, draft privacy, publishing, edit, categories, page bounds, duplicate slug, invalid upload, unpublishing.");
}
finally
{
    await transaction.RollbackAsync();
    Directory.Delete(environment.WebRootPath, recursive: true);
}

using var client = new HttpClient(new HttpClientHandler { AllowAutoRedirect = false });
foreach (var path in new[] { "/articles", "/articles/sewing-essentials", "/articles/thread-and-needle", "/articles/ribbon-and-lace", "/blog" })
    Assert((int)(await client.GetAsync("http://localhost:5180" + path)).StatusCode == 200, "Public route failed: " + path);
Assert((int)(await client.GetAsync("http://localhost:5180/articles/no-such-article")).StatusCode == 404, "Missing article did not return 404.");
Assert((int)(await client.GetAsync("http://localhost:5180/Admin/Articles/create")).StatusCode == 302, "Admin route is not protected.");
Assert((int)(await client.GetAsync("http://localhost:5180/articles?page=-50")).StatusCode == 200, "Negative page failed.");
Console.WriteLine("PASS: public routes, missing article 404, anonymous admin redirect, negative page.");

sealed class TestEnvironment(string contentRoot, string webRoot) : IWebHostEnvironment
{
    public string ApplicationName { get; set; } = "lern";
    public string EnvironmentName { get; set; } = "Development";
    public string ContentRootPath { get; set; } = contentRoot;
    public IFileProvider ContentRootFileProvider { get; set; } = new NullFileProvider();
    public string WebRootPath { get; set; } = webRoot;
    public IFileProvider WebRootFileProvider { get; set; } = new NullFileProvider();
}
sealed class TestTempDataProvider : ITempDataProvider
{
    public IDictionary<string, object> LoadTempData(HttpContext context) => new Dictionary<string, object>();
    public void SaveTempData(HttpContext context, IDictionary<string, object> values) { }
}
