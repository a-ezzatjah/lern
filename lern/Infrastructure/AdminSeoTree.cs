using Entities;
using lern.Models;
using Microsoft.EntityFrameworkCore;

namespace lern.Infrastructure;

public sealed class AdminSeoTree(ShopDbContext db, SeoCatalog catalog)
{
    public const int PageSize = 10;

    public async Task<List<AdminSeoNode>> Build()
    {
        var entries = (await catalog.Entries()).ToDictionary(x => x.Path);
        AdminSeoNode Page(SeoEntry entry) => new() { Key = entry.Path, Title = entry.Title, Kind = "page", Entry = entry };
        var categories = await db.Categories.AsNoTracking().OrderBy(x => x.SortOrder).ThenBy(x => x.Name).ThenBy(x => x.Id)
            .Select(x => new { x.Id, x.Name, x.ParentId }).ToListAsync();
        var byParent = categories.ToLookup(x => x.ParentId);
        var byId = categories.ToDictionary(x => x.Id);
        var ids = categories.Select(x => x.Id).ToHashSet();
        var visited = new HashSet<int>();
        AdminSeoNode Category(int id)
        {
            visited.Add(id);
            var category = byId[id];
            var children = new List<AdminSeoNode>();
            foreach (var child in byParent[id])
                if (!visited.Contains(child.Id)) children.Add(Category(child.Id));
            return new AdminSeoNode { Key = "category:" + id, Title = category.Name, Kind = "category",
                Entry = entries[$"/shop?category={id}"], Children = children };
        }
        var categoryRoots = new List<AdminSeoNode>();
        foreach (var category in categories.Where(x => x.ParentId is null || !ids.Contains(x.ParentId.Value)))
            if (!visited.Contains(category.Id)) categoryRoots.Add(Category(category.Id));
        // Preserve access even if an old category contains an invalid parent cycle.
        foreach (var category in categories)
            if (!visited.Contains(category.Id)) categoryRoots.Add(Category(category.Id));

        var articles = await db.Articles.AsNoTracking().Where(x => x.IsPublished).OrderBy(x => x.Title).ThenBy(x => x.Id)
            .Select(x => new { x.Slug, x.Category }).ToListAsync();
        var articleGroups = articles.GroupBy(x => x.Category).OrderBy(x => x.Key).Select(group => new AdminSeoNode
        {
            Key = "article-category:" + group.Key, Title = group.Key, Kind = "article-category",
            Children = group.Select(x => Page(entries["/articles/" + x.Slug])).ToList()
        }).ToList();
        return [
            new() { Key = "home", Title = "صفحه اصلی", Kind = "home", Children = [Page(entries["/"])] },
            new() { Key = "products", Title = "محصولات", Kind = "products", Entry = entries["/shop"],
                Children = entries.Values.Where(x => x.Path.StartsWith("/product/")).OrderBy(x => x.Title).ThenBy(x => x.Path).Select(Page).ToList() },
            new() { Key = "articles", Title = "مقالات", Kind = "articles", Entry = entries["/articles"], Children = articleGroups },
            new() { Key = "categories", Title = "دسته‌بندی محصولات", Kind = "categories", Children = categoryRoots }
        ];
    }

    public static AdminSeoNode? Find(IEnumerable<AdminSeoNode> nodes, string key)
    {
        foreach (var node in nodes)
        {
            if (node.Key == key) return node;
            var child = Find(node.Children, key);
            if (child is not null) return child;
        }
        return null;
    }

    // Search the complete tree before pagination so unloaded pages remain searchable.
    public static List<AdminSeoNode> Filter(IEnumerable<AdminSeoNode> nodes, string? query)
    {
        if (string.IsNullOrWhiteSpace(query)) return nodes.ToList();
        var term = Normalize(query.Trim());
        var result = new List<AdminSeoNode>();
        foreach (var node in nodes)
        {
            if (Normalize(node.Title).Contains(term, StringComparison.OrdinalIgnoreCase) ||
                Normalize(node.PreviewPath ?? "").Contains(term, StringComparison.OrdinalIgnoreCase))
            {
                result.Add(node);
                continue;
            }
            var children = Filter(node.Children, query);
            if (children.Count > 0)
                result.Add(new AdminSeoNode { Key = node.Key, Title = node.Title, Kind = node.Kind, Entry = node.Entry, Children = children });
        }
        return result;
    }

    private static string Normalize(string value) => value.Replace('ي', 'ی').Replace('ك', 'ک').Replace('\u200c', ' ');

    public static AdminSeoChildrenViewModel Children(AdminSeoNode node, int offset) =>
        new(node.Key, node.Children.Skip(offset).Take(PageSize).ToList(), Math.Min(node.Children.Count, offset + PageSize), node.Children.Count);
}
