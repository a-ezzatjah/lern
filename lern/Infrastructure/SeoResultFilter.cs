using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.EntityFrameworkCore;
using Entities;

namespace lern.Infrastructure;

public class SeoResultFilter(SeoCatalog catalog, ShopDbContext db) : IAsyncResultFilter
{
    public async Task OnResultExecutionAsync(ResultExecutingContext context, ResultExecutionDelegate next)
    {
        if (context.Result is ViewResult view && context.Controller is Microsoft.AspNetCore.Mvc.Controller controller)
        {
            var request = context.HttpContext.Request;
            var path = request.Path.Value?.TrimEnd('/') ?? "";
            if (path == "") path = "/";
            if (path == "/blog") path = "/articles";
            var filtered = false;
            if (path == "/shop")
            {
                var ids = request.Query["category"];
                if (ids.Count == 1 && int.TryParse(ids[0], out var id)) path += "?category=" + id;
                filtered = request.Query.Any(x => x.Key != "category") || ids.Count > 1;
            }
            else if (path == "/articles") filtered = request.Query.Count > 0;
            var data = view.ViewData ?? controller.ViewData;
            var publicPage = path is "/" or "/shop" or "/articles" || StoreSupportContent.Pages.Any(x => x.Path == path) || path.StartsWith("/shop?category=") ||
                path.StartsWith("/product/") || path.StartsWith("/articles/");
            if (publicPage)
            {
                var settings = await db.SeoPages.AsNoTracking().SingleOrDefaultAsync(x => x.Path == path);
                if (settings is not null && !path.StartsWith("/product/") && !path.StartsWith("/shop?category="))
                {
                    if (!string.IsNullOrWhiteSpace(settings.MetaTitle)) data["Title"] = settings.MetaTitle;
                    if (!string.IsNullOrWhiteSpace(settings.MetaDescription)) data["MetaDescription"] = settings.MetaDescription;
                    data["Robots"] = $"{(settings.IndexPage ? "index" : "noindex")}, {(settings.FollowPage ? "follow" : "nofollow")}";
                }
                data["Robots"] ??= "index, follow";
                if (filtered) data["Robots"] = "noindex, follow";
                if (string.IsNullOrWhiteSpace(data["CanonicalUrl"]?.ToString())) data["CanonicalUrl"] = catalog.Origin(request) + path;
            }
            else data["Robots"] = "noindex, nofollow";
            context.HttpContext.Response.Headers["X-Robots-Tag"] = data["Robots"]?.ToString();
        }
        await next();
    }
}
