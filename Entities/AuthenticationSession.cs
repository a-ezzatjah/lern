namespace Entities;

public sealed class AuthenticationSession
{
    public string KeyHash { get; set; } = null!;
    public int UserId { get; set; }
    public CustomerUser User { get; set; } = null!;
    public Guid AuthenticationVersion { get; set; }
    public string Role { get; set; } = null!;
    public byte[] ProtectedTicket { get; set; } = null!;
    public DateTime CreatedAt { get; set; }
    public DateTime LastSeenAt { get; set; }
    public DateTime ExpiresAt { get; set; }
}
