using System.Net;
using System.Text.RegularExpressions;
using AngleSharp.Html.Parser;
using Ganss.Xss;

namespace lern.Infrastructure;

public static class RichText
{
    public static string Render(string? value)
    {
        if (string.IsNullOrWhiteSpace(value)) return "";
        // Preserve existing plain text and the original article heading convention.
        if (!Regex.IsMatch(value, @"</?(?:p|div|h[1-6]|strong|b|a|br|ul|ol|li|em|i|blockquote|script|img)\b", RegexOptions.IgnoreCase))
            value = string.Join("", value.Replace("\r\n", "\n").Split("\n\n", StringSplitOptions.RemoveEmptyEntries)
                .Select(p => p.StartsWith("## ") ? "<h2>" + WebUtility.HtmlEncode(p[3..]) + "</h2>" : "<p>" + WebUtility.HtmlEncode(p).Replace("\n", "<br>") + "</p>"));
        var sanitizer = new HtmlSanitizer();
        sanitizer.AllowedTags.Clear();
        sanitizer.AllowedTags.UnionWith(new[] { "p", "div", "span", "br", "h1", "h2", "h3", "h4", "h5", "h6", "strong", "b", "em", "i", "u", "a", "ul", "ol", "li", "blockquote" });
        sanitizer.AllowedAttributes.Clear();
        sanitizer.AllowedAttributes.UnionWith(new[] { "href", "title", "rel", "target" });
        sanitizer.AllowedSchemes.Clear();
        sanitizer.AllowedSchemes.UnionWith(new[] { "http", "https" });
        var document = new HtmlParser().ParseDocument(sanitizer.Sanitize(value));
        foreach (var link in document.QuerySelectorAll("a"))
        {
            var href = link.GetAttribute("href") ?? "";
            if (href.StartsWith("//") || href.Contains('\\') || href.Any(char.IsControl)) link.RemoveAttribute("href");
            var rel = (link.GetAttribute("rel") ?? "").Split(' ', StringSplitOptions.RemoveEmptyEntries)
                .Where(x => x is "nofollow" or "sponsored" or "ugc").ToList();
            if (link.GetAttribute("target") == "_blank") rel.AddRange(new[] { "noopener", "noreferrer" });
            else link.RemoveAttribute("target");
            link.SetAttribute("rel", string.Join(" ", rel.Distinct()));
        }
        return document.Body!.InnerHtml;
    }

    public static bool HasText(string? value) => !string.IsNullOrWhiteSpace(new HtmlParser().ParseDocument(Render(value)).Body?.TextContent);

    public static string Summary(string? value, int maxLength = 160)
    {
        var html = Regex.Replace(Render(value), @"</(?:p|div|h[1-6]|li|blockquote)>|<br\s*/?>", " ", RegexOptions.IgnoreCase);
        var text = Regex.Replace(new HtmlParser().ParseDocument(html).Body!.TextContent, @"\s+", " ").Trim();
        var limit = Math.Max(4, maxLength);
        return text.Length > limit ? text[..(limit - 3)] + "…" : text;
    }
}
