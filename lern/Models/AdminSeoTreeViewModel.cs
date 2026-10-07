using lern.Infrastructure;

namespace lern.Models;

public sealed class AdminSeoNode
{
    public string Key { get; init; } = "";
    public string Title { get; init; } = "";
    public string Kind { get; init; } = "folder";
    public SeoEntry? Entry { get; init; }
    public List<AdminSeoNode> Children { get; init; } = [];
    public string? PreviewPath => Entry?.Path ?? (Kind == "article-category" ? "/articles?category=" + Uri.EscapeDataString(Title) : null);
}

public sealed record AdminSeoChildrenViewModel(string Key, IReadOnlyList<AdminSeoNode> Nodes, int NextOffset, int Total)
{
    public bool HasMore => NextOffset < Total;
}
