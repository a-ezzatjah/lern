using Entities;
using lern.Models;
using Microsoft.EntityFrameworkCore;

namespace lern.Infrastructure;

public sealed class AdminCategorySeoTree(ShopDbContext db)
{
    public async Task<List<AdminSeoNode>> Build()
    {
        // Descriptions and SEO fields are fetched separately when an editor opens.
        var categories = await db.Categories.AsNoTracking().OrderBy(x => x.SortOrder).ThenBy(x => x.Name).ThenBy(x => x.Id)
            .Select(x => new { x.Id, x.Name, x.ParentId }).ToListAsync();
        var byId = categories.ToDictionary(x => x.Id);
        var byParent = categories.ToLookup(x => x.ParentId);
        var visited = new HashSet<int>();
        AdminSeoNode Node(int id)
        {
            visited.Add(id);
            var category = byId[id];
            var children = new List<AdminSeoNode>();
            foreach (var child in byParent[id])
                if (!visited.Contains(child.Id)) children.Add(Node(child.Id));
            return new AdminSeoNode { Key = "category:" + id, Title = category.Name, Kind = "category-editor",
                Entry = new SeoEntry($"/shop?category={id}", category.Name), Children = children };
        }
        var roots = new List<AdminSeoNode>();
        foreach (var category in categories.Where(x => x.ParentId is null || !byId.ContainsKey(x.ParentId.Value)))
            if (!visited.Contains(category.Id)) roots.Add(Node(category.Id));
        // Keep malformed old category relationships accessible without recursion cycles.
        foreach (var category in categories)
            if (!visited.Contains(category.Id)) roots.Add(Node(category.Id));
        return [new AdminSeoNode { Key = "categories", Title = "دسته‌بندی‌ها", Kind = "categories", Children = roots }];
    }
}
