using Entities;

namespace lern.Models;

public sealed class AdminProductListViewModel
{
    public string? Search { get; set; }
    public IReadOnlyList<Product> Products { get; init; } = Array.Empty<Product>();
    public int Page { get; init; } = 1;
    public int PageSize { get; init; } = 10;
    public int TotalCount { get; init; }
    public int TotalPages => Math.Max(1, (int)Math.Ceiling(TotalCount / (double)PageSize));
}
