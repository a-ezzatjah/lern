using System.Xml.Linq;
using lern.Infrastructure;
using Microsoft.AspNetCore.Mvc;

namespace lern.Controller;

public class SitemapController(SeoCatalog catalog) : Microsoft.AspNetCore.Mvc.Controller
{
    private const int PageSize = 1000;
    private static readonly XNamespace Ns = "http://www.sitemaps.org/schemas/sitemap/0.9";

    [HttpGet("/sitemap.xml")]
    public async Task<IActionResult> Index()
    {
        var pages = (await catalog.Entries()).Where(x => x.IsInSitemap).ToList();
        var origin = catalog.Origin(Request);
        if (pages.Count <= PageSize) return Xml(Urls(pages, origin));
        return Xml(new XElement(Ns + "sitemapindex", Enumerable.Range(1, (int)Math.Ceiling(pages.Count / (double)PageSize))
            .Select(i => new XElement(Ns + "sitemap", new XElement(Ns + "loc", $"{origin}/sitemap-{i}.xml")))));
    }

    [HttpGet("/sitemap-{page:int}.xml")]
    public async Task<IActionResult> Page(int page)
    {
        var pages = (await catalog.Entries()).Where(x => x.IsInSitemap).ToList();
        if (page < 1 || page > Math.Ceiling(pages.Count / (double)PageSize)) return NotFound();
        return Xml(Urls(pages.Skip((page - 1) * PageSize).Take(PageSize), catalog.Origin(Request)));
    }

    [HttpGet("/robots.txt")]
    public IActionResult Robots() => Content($"User-agent: *\nAllow: /\nSitemap: {catalog.Origin(Request)}/sitemap.xml\n", "text/plain; charset=utf-8");

    private static XElement Urls(IEnumerable<SeoEntry> entries, string origin) => new(Ns + "urlset",
        entries.Select(x => new XElement(Ns + "url", new XElement(Ns + "loc", origin + x.Path))));
    private ContentResult Xml(XElement root) => Content(new XDocument(new XDeclaration("1.0", "utf-8", null), root).ToString(), "application/xml; charset=utf-8");
}
