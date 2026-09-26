namespace Entities;
public sealed class ProductFavorite
{
    public int UserId { get; set; }
    public CustomerUser User { get; set; } = null!;
    public int ProductId { get; set; }
    public Product Product { get; set; } = null!;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}
