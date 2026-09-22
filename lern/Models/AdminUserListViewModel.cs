using Entities;

namespace lern.Models;

public sealed class AdminUserListViewModel
{
    public string? Search { get; init; }
    public IReadOnlyList<CustomerUser> Users { get; init; } = Array.Empty<CustomerUser>();
    public int Page { get; init; } = 1;
    public int PageSize { get; init; } = 10;
    public int TotalCount { get; init; }
    public int TotalPages => Math.Max(1, (int)Math.Ceiling(TotalCount / (double)PageSize));
}
