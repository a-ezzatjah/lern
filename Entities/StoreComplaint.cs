namespace Entities;

public enum ComplaintSubject { Order = 1, Product = 2, Delivery = 3, Payment = 4, Other = 5 }

public sealed class StoreComplaint
{
    public int Id { get; set; }
    public string Reference { get; set; } = Guid.NewGuid().ToString("N");
    public string FullName { get; set; } = "";
    public string Phone { get; set; } = "";
    public string? OrderNumber { get; set; }
    public ComplaintSubject Subject { get; set; }
    public string Message { get; set; } = "";
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public bool IsReviewed { get; set; }
}
