using System.ComponentModel.DataAnnotations;
using System.Net;
using System.Text.Json;
using System.Text.RegularExpressions;
using System.Xml.Linq;
using Entities;
using lern.Controller;
using lern.Infrastructure;
using lern.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;

void Check(bool passed, string name) { if (!passed) throw new Exception(name); Console.WriteLine("PASS " + name); }
bool Valid(ComplaintViewModel model) => Validator.TryValidateObject(model, new ValidationContext(model), [], true);
ComplaintViewModel Sample() => new() { FullName = "  Support Smoke  ", Phone = "۰۹۱۲۳۴۵۶۷۸۹", OrderNumber = "١٢٣", Subject = ComplaintSubject.Product, Message = "  درخواست آزمایشی برای بررسی ثبت و رسیدگی در تست  " };
var sample = Sample();
Check(Valid(sample) && sample.Phone == "09123456789" && sample.OrderNumber == "123" && sample.FullName == "Support Smoke", "normalizes Persian/Arabic digits and trims text");
var invalid = Sample(); invalid.Phone = "123"; Check(!Valid(invalid), "rejects invalid mobile");
invalid = Sample(); invalid.Subject = (ComplaintSubject)99; Check(!Valid(invalid), "rejects unknown subject");
invalid = Sample(); invalid.Message = "     "; Check(!Valid(invalid), "rejects blank message");
invalid = Sample(); invalid.Message = new string('a', 3001); Check(!Valid(invalid), "limits message length");
invalid = Sample(); invalid.OrderNumber = "abc"; Check(!Valid(invalid), "rejects invalid order number");

using var settings = JsonDocument.Parse(await File.ReadAllTextAsync("lern/appsettings.json"));
var connection = settings.RootElement.GetProperty("ConnectionStrings").GetProperty("DefaultConnectionstring").GetString();
await using var db = new ShopDbContext(new DbContextOptionsBuilder<ShopDbContext>().UseSqlServer(connection).Options);
var before = await db.StoreComplaints.CountAsync();
await using (var transaction = await db.Database.BeginTransactionAsync())
{
    try
    {
        var context = new DefaultHttpContext();
        var controller = new StoreSupportController(db) { ControllerContext = new ControllerContext { HttpContext = context }, TempData = new TempDataDictionary(context, new TestTempDataProvider()) };
        var result = await controller.SubmitComplaint(sample, default);
        var reference = controller.TempData["ComplaintReference"] as string;
        Check(result is RedirectToActionResult { ActionName: "Complaints" } && reference?.Length == 32, "submission redirects and returns private tracking reference");
        var saved = await db.StoreComplaints.SingleAsync(x => x.Reference == reference);
        Check(saved.Phone == sample.Phone && saved.FullName == sample.FullName && !saved.IsReviewed && saved.Message == sample.Message, "complaint persists normalized data");
        var admin = new AdminComplaintsController(db) { ControllerContext = new ControllerContext { HttpContext = context } };
        var list = (ViewResult)await admin.Index(-1, false);
        Check(((IReadOnlyList<StoreComplaint>)list.Model!).Any(x => x.Id == saved.Id) && (int)admin.ViewData["Page"]! == 1, "admin pending list includes complaint and clamps page");
        Check(await admin.Review(saved.Id) is RedirectToActionResult && saved.IsReviewed, "admin can mark complaint reviewed");
        list = (ViewResult)await admin.Index(1, false);
        Check(!((IReadOnlyList<StoreComplaint>)list.Model!).Any(x => x.Id == saved.Id), "reviewed complaint leaves pending filter");
        Check(await admin.Review(int.MaxValue) is NotFoundResult, "missing complaint returns 404");
        controller.ModelState.AddModelError("Phone", "invalid");
        Check(await controller.SubmitComplaint(sample, default) is ViewResult && await db.StoreComplaints.CountAsync() == before + 1, "invalid submission redisplays form without saving");
        controller.ModelState.Clear(); sample.Website = "bot";
        Check(await controller.SubmitComplaint(sample, default) is BadRequestResult, "honeypot blocks automated submission");
    }
    finally { await transaction.RollbackAsync(); }
}
db.ChangeTracker.Clear();
Check(await db.StoreComplaints.CountAsync() == before, "all test complaints rolled back");
var catalog = new SeoCatalog(db, new ConfigurationBuilder().Build());
var supportBranch = (await new AdminSeoTree(db, catalog).Build()).Single(x => x.Key == "support");
Check(supportBranch.Kind == "folder" && supportBranch.Children.Count == 4 && supportBranch.Children.All(x => x.Entry?.MetaDescription is { Length: > 0 }), "support SEO folder exposes four editable pages with default descriptions");

using var handler = new HttpClientHandler { AllowAutoRedirect = false, CookieContainer = new CookieContainer() };
using var http = new HttpClient(handler) { BaseAddress = new Uri("http://localhost:5180") };
foreach (var page in StoreSupportContent.Pages)
{
    using var response = await http.GetAsync(page.Path);
    var html = await response.Content.ReadAsStringAsync();
    Check(response.IsSuccessStatusCode && html.Contains("ks-footer") && html.Contains("<h1>"), page.Path + " renders shared footer and heading");
    Check(response.Headers.GetValues("X-Robots-Tag").Single() == "index, follow" && html.Contains("rel=\"canonical\"") && html.Contains("property=\"og:description\""), page.Path + " canonical, robots and Open Graph metadata");
    var json = Regex.Match(html, "<script type=\"application/ld\\+json\">(.*?)</script>", RegexOptions.Singleline).Groups[1].Value;
    using var structured = JsonDocument.Parse(json);
    Check(structured.RootElement.GetProperty("@graph").GetArrayLength() >= 3, page.Path + " valid structured data");
}
var sitemap = XDocument.Parse(await http.GetStringAsync("/sitemap.xml"));
XNamespace ns = "http://www.sitemaps.org/schemas/sitemap/0.9";
var locations = sitemap.Descendants(ns + "loc").Select(x => x.Value).ToList();
if (sitemap.Root?.Name.LocalName == "sitemapindex")
{
    var maps = locations.ToArray(); locations.Clear();
    foreach (var map in maps) locations.AddRange(XDocument.Parse(await http.GetStringAsync(map)).Descendants(ns + "loc").Select(x => x.Value));
}
Check(StoreSupportContent.Pages.All(x => locations.Any(url => url.EndsWith(x.Path))), "all support pages included in sitemap");
using (var response = await http.GetAsync("/Admin/Settings/complaints"))
    Check(response.StatusCode == HttpStatusCode.Redirect && response.Headers.Location?.OriginalString.Contains("/login") == true, "anonymous visitors cannot access complaint inbox");
using (var response = await http.PostAsync("/complaints", new FormUrlEncodedContent(new Dictionary<string, string>())))
    Check(response.StatusCode == HttpStatusCode.BadRequest, "missing anti-forgery token rejected");
var form = await http.GetStringAsync("/complaints");
var token = WebUtility.HtmlDecode(Regex.Match(form, "name=\"__RequestVerificationToken\"[^>]*value=\"([^\"]+)\"").Groups[1].Value);
Check(token.Length > 0, "complaint form includes anti-forgery token");
var body = new Dictionary<string, string> { ["__RequestVerificationToken"] = token, ["FullName"] = "Test", ["Phone"] = "123", ["Subject"] = "99", ["Message"] = "short" };
using (var response = await http.PostAsync("/complaints", new FormUrlEncodedContent(body)))
{
    var html = await response.Content.ReadAsStringAsync();
    Check(response.StatusCode == HttpStatusCode.OK && html.Contains("field-validation-error") && await db.StoreComplaints.CountAsync() == before, "HTTP invalid form returns field errors without creating complaint");
}
for (var attempt = 0; attempt < 5; attempt++)
{
    using var response = await http.PostAsync("/complaints", new FormUrlEncodedContent(body));
    if (response.StatusCode == HttpStatusCode.TooManyRequests)
    { Check(response.Headers.RetryAfter is not null, "complaint rate limit rejects excessive submissions"); break; }
    if (attempt == 4) throw new Exception("Rate limit did not reject excessive submissions");
}
Console.WriteLine("Store support smoke checks passed. No complaint test data retained.");

sealed class TestTempDataProvider : ITempDataProvider
{
    public IDictionary<string, object> LoadTempData(HttpContext context) => new Dictionary<string, object>();
    public void SaveTempData(HttpContext context, IDictionary<string, object> values) { }
}
