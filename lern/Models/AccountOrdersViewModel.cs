using Entities;

namespace lern.Models;

public sealed class AccountOrdersViewModel
{
    public List<Order> Orders { get; init; } = new();
    public OrderStatus? Status { get; init; }
    public string? Period { get; init; }
    public string? Amount { get; init; }
    public int Page { get; init; }
    public int TotalCount { get; init; }
    public int PageSize { get; init; } = 10;
    public int TotalPages => (TotalCount + PageSize - 1) / PageSize;
}
