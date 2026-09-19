namespace Entities;

public class ProductViewHistory
{
    public int Id { get; set; }
    public int ProductId { get; set; }
    public Product Product { get; set; } = null!;
    public string CustomerKey { get; set; } = null!;
    public DateTime LastViewedAt { get; set; } = DateTime.UtcNow;
}
