using Entities;

namespace lern.Models;

public sealed class AdminCategoryListViewModel
{
    public string? Search { get; init; }
    public IReadOnlyList<Category> Categories { get; init; } = Array.Empty<Category>();
    public IReadOnlyList<Category> ParentOptions { get; init; } = Array.Empty<Category>();
    public int Page { get; init; }
    public int PageSize { get; init; }
    public int TotalCount { get; init; }
    public int TotalPages => Math.Max(1, (int)Math.Ceiling(TotalCount / (double)PageSize));
}
