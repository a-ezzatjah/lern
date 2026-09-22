using Entities;

namespace lern.Models;

public sealed class AdminCategoryListViewModel
{
    public IReadOnlyList<Category> Categories { get; init; } = Array.Empty<Category>();
    public int TotalCount { get; init; }
}
