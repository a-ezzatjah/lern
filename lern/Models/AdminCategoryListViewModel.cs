using Entities;

namespace lern.Models;

public sealed class AdminCategoryListViewModel
{
    public string? Search { get; init; }
    public IReadOnlyList<Category> Categories { get; init; } = Array.Empty<Category>();
    public IReadOnlyList<Category> ParentOptions { get; init; } = Array.Empty<Category>();
}
