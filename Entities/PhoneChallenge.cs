namespace Entities;

// One row per phone keeps resend/attempt budgets shared across both flows and app replicas.
public sealed class PhoneChallenge
{
    public string PhoneNumber { get; set; } = null!;
    public Guid ChallengeId { get; set; }
    public string Purpose { get; set; } = null!;
    public string CodeHash { get; set; } = "";
    public string Salt { get; set; } = "";
    public int? UserId { get; set; }
    public Guid AuthenticationVersion { get; set; }
    public DateTime ExpiresAt { get; set; }
    public DateTime LastSentAt { get; set; }
    public DateTime WindowStartedAt { get; set; }
    public int SendCount { get; set; }
    public int AttemptCount { get; set; }
    public bool Consumed { get; set; }
}
