namespace Entities;

public class Address
{
    public int Id { get; set; }
    public string CustomerKey { get; set; } = null!;
    public string Title { get; set; } = null!;
    public string Province { get; set; } = null!;
    public string City { get; set; } = null!;
    public string Details { get; set; } = null!;
    public string PostalCode { get; set; } = null!;
    public string Phone { get; set; } = null!;
    public string ReceiverName { get; set; } = null!;
    public bool IsDefault { get; set; }
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
}
