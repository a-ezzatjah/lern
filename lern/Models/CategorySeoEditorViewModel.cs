using Entities;
using lern.Infrastructure;

namespace lern.Models;

public sealed record CategorySeoEditorViewModel(Category Category, bool IncludeInSitemap)
{
    public SeoEntry Entry => new($"/shop?category={Category.Id}", Category.Name, Category.Seo?.IndexPage ?? true,
        Category.Seo?.FollowPage ?? true, IncludeInSitemap && string.IsNullOrWhiteSpace(Category.Seo?.CanonicalUrl),
        Category.Seo?.MetaTitle, Category.Seo?.MetaDescription);
}
