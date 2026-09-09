using Entities;

namespace lern.Models;

public sealed class AdminProductListViewModel
{
    public string? Search { get; set; }
    public IReadOnlyList<Product> Products { get; init; } = Array.Empty<Product>();
}
