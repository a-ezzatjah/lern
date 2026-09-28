using Entities;

namespace lern.Models;

public sealed class AdminOrderDetailsViewModel
{
    public required Order Order { get; init; }
    public Address? CurrentAddress { get; init; }
    public string? CustomerEmail { get; init; }
}
