using Entities;

namespace lern.Models;

public sealed class AdminTransactionListViewModel
{
    public List<PaymentTransaction> Transactions { get; init; } = new();
    public string? Search { get; init; }
    public PaymentStatus? Status { get; init; }
    public string? Type { get; init; }
    public DateTime? From { get; init; }
    public DateTime? To { get; init; }
    public int Page { get; init; }
    public int PageSize { get; init; } = 10;
    public int TotalCount { get; init; }
    public int FailedCount { get; init; }
    public decimal SuccessfulAmount { get; init; }
    public decimal AverageAmount { get; init; }
    public int TotalPages => Math.Max(1, (int)Math.Ceiling(TotalCount / (double)PageSize));
}
