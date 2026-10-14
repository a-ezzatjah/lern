namespace Entities;

public sealed class SiteIntegrationSettings
{
    public int Id { get; set; } = 1;
    public bool SmsEnabled { get; set; }
    public string SmsProvider { get; set; } = "Kavenegar";
    public string? SmsApiKeyProtected { get; set; }
    public string? SmsSender { get; set; }
    public string? SmsApiUrl { get; set; }
    public string? SmsApiKeyHeader { get; set; }
    public string SmsMessageTemplate { get; set; } = "کد تأیید فروشگاه: {code}";
    public bool PaymentEnabled { get; set; }
    public string PaymentGateway { get; set; } = "Zarinpal";
    public string? PaymentMerchantIdProtected { get; set; }
    public string? PaymentRequestUrl { get; set; }
    public string? PaymentVerifyUrl { get; set; }
    public string? PaymentCheckoutUrlTemplate { get; set; }
    public string? PaymentApiKeyHeader { get; set; }
    public bool EnamadEnabled { get; set; }
    public string? EnamadCode { get; set; }
    public string? EnamadLogoUrl { get; set; }
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
}
