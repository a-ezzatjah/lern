namespace Entities;

public enum SiteInboxKind { Notification = 1, Message = 2 }

public class SiteInboxItem
{
    public int Id { get; set; }
    public SiteInboxKind Kind { get; set; }
    public string Title { get; set; } = "";
    public string Body { get; set; } = "";
    // Null means all account holders, including accounts created later.
    public int? RecipientUserId { get; set; }
    public CustomerUser? RecipientUser { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}
