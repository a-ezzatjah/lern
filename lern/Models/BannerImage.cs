using Entities;

namespace lern.Models;

public static class BannerImage
{
    // Slider art was re-encoded from heavy PNG to JPEG. If a lighter sibling
    // file exists on disk, prefer it so the site stays fast even when the
    // database still points at the old PNG path.
    public static string Resolve(string? url, IWebHostEnvironment env)
    {
        if (string.IsNullOrWhiteSpace(url)) return url ?? string.Empty;
        if (!url.EndsWith(".png", StringComparison.OrdinalIgnoreCase)) return url;

        var candidate = url[..^4] + ".jpg";
        var physical = Path.Combine(env.WebRootPath ?? "wwwroot", candidate.TrimStart('/').Replace('/', Path.DirectorySeparatorChar));
        return File.Exists(physical) ? candidate : url;
    }

    public static void Apply(SiteBanner banner, IWebHostEnvironment env)
    {
        banner.ImageUrl = Resolve(banner.ImageUrl, env);
        banner.MobileImageUrl = Resolve(banner.MobileImageUrl, env);
    }
}
