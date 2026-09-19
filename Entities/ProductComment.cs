namespace Entities;

/// <summary>
/// A customer's written review for a product. Reviews are published only after approval.
/// </summary>
public class ProductComment
{
    public int Id { get; set; }
    public int ProductId { get; set; }
    public Product Product { get; set; } = null!;
    public string CustomerKey { get; set; } = null!;
    public string AuthorName { get; set; } = null!;
    public string? Title { get; set; }
    public string Body { get; set; } = null!;
    public bool IsApproved { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
}
