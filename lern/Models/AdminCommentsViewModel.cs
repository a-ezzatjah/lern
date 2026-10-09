using Entities;
namespace lern.Models;

public sealed class AdminCommentsViewModel
{
    public const int PageSize = 10;
    public List<ProductComment> Comments { get; init; } = new();
    public string? Query { get; init; }
    public string Status { get; init; } = "pending";
    public int PendingCount { get; init; }
    public int ApprovedCount { get; init; }
    public int BlockedCount { get; init; }
    public int TotalCount { get; init; }
    public int Page { get; init; }
    public int TotalPages => Math.Max(1, (TotalCount + PageSize - 1) / PageSize);
}
