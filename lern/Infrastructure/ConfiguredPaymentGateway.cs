using System.Net.Http.Json;
using System.Security.Cryptography;
using System.Text.Json;
using Entities;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.EntityFrameworkCore;
using ServiceContract.Interfaces;

namespace lern.Infrastructure;

public sealed class ConfiguredPaymentGateway(
    ShopDbContext db,
    IHttpClientFactory clients,
    IDataProtectionProvider protectionProvider) : IOrderPaymentGateway
{
    private readonly IDataProtector protector = protectionProvider.CreateProtector("lern.admin.integration-secrets.v1");
    private const string ZarinpalApi = "https://payment.zarinpal.com/pg/v4/payment";
    public string Name => "zarinpal";

    public bool IsConfigured => Load().GetAwaiter().GetResult() is { PaymentEnabled: true, PaymentMerchantIdProtected: not null };

    public async Task<GatewayStartResult> StartAsync(int orderId, decimal amountInToman, string callbackUrl, string phone, CancellationToken cancellationToken = default)
    {
        var merchant = await MerchantAsync();
        if (merchant is null) return new(false, Error: "درگاه پرداخت تنظیم نشده است.");
        var client = clients.CreateClient("integrations");
        using var response = await client.PostAsJsonAsync($"{ZarinpalApi}/request.json", new
        {
            merchant_id = merchant,
            amount = checked((long)Math.Round(amountInToman * 10m, MidpointRounding.AwayFromZero)),
            callback_url = callbackUrl,
            description = $"پرداخت سفارش {orderId}",
            metadata = new { mobile = phone }
        }, cancellationToken);
        if (!response.IsSuccessStatusCode) return new(false, Error: "ارتباط با درگاه برقرار نشد.");
        using var json = await JsonDocument.ParseAsync(await response.Content.ReadAsStreamAsync(cancellationToken), cancellationToken: cancellationToken);
        var data = json.RootElement.GetProperty("data");
        var code = data.GetProperty("code").GetInt32();
        if (code != 100) return new(false, Error: $"درگاه پرداخت درخواست را نپذیرفت ({code}).");
        var authority = data.GetProperty("authority").GetString();
        return string.IsNullOrWhiteSpace(authority) ? new(false, Error: "درگاه شناسه پرداخت معتبری برنگرداند.") : new(true, authority, $"https://www.zarinpal.com/pg/StartPay/{Uri.EscapeDataString(authority)}");
    }

    public async Task<GatewayVerifyResult> VerifyAsync(string authority, decimal amountInToman, CancellationToken cancellationToken = default)
    {
        var merchant = await MerchantAsync();
        if (merchant is null) return new(false, Error: "درگاه پرداخت تنظیم نشده است.");
        var client = clients.CreateClient("integrations");
        using var response = await client.PostAsJsonAsync($"{ZarinpalApi}/verify.json", new
        {
            merchant_id = merchant,
            amount = checked((long)Math.Round(amountInToman * 10m, MidpointRounding.AwayFromZero)),
            authority
        }, cancellationToken);
        if (!response.IsSuccessStatusCode) return new(false, Error: "ارتباط با درگاه برقرار نشد.");
        using var json = await JsonDocument.ParseAsync(await response.Content.ReadAsStreamAsync(cancellationToken), cancellationToken: cancellationToken);
        var data = json.RootElement.GetProperty("data");
        var code = data.GetProperty("code").GetInt32();
        var reference = data.TryGetProperty("ref_id", out var refId) ? refId.ToString() : null;
        return code is 100 or 101 && !string.IsNullOrWhiteSpace(reference)
            ? new(true, reference)
            : new(false, Error: $"پرداخت تأیید نشد ({code}).", DefinitiveFailure: code is not 101);
    }

    private async Task<string?> MerchantAsync()
    {
        var settings = await Load();
        if (settings is not { PaymentEnabled: true, PaymentMerchantIdProtected: not null }) return null;
        try { return protector.Unprotect(settings.PaymentMerchantIdProtected); }
        catch (Exception ex) when (ex is CryptographicException or ArgumentException) { return null; }
    }
    private Task<SiteIntegrationSettings?> Load() => db.SiteIntegrationSettings.AsNoTracking().SingleOrDefaultAsync(x => x.Id == 1);
}
