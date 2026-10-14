using System.Net.Http.Json;
using System.Security.Cryptography;
using System.Text.Json;
using Entities;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.EntityFrameworkCore;
using ServiceContract.Interfaces;

namespace lern.Infrastructure;

public sealed class ConfiguredPhoneCodeSender(
    ShopDbContext db,
    IHttpClientFactory clients,
    IDataProtectionProvider protectionProvider) : IPhoneCodeSender
{
    private readonly IDataProtector protector = protectionProvider.CreateProtector("lern.admin.integration-secrets.v1");

    public bool IsConfigured => Load().GetAwaiter().GetResult() is { SmsEnabled: true, SmsApiKeyProtected: not null };

    public async Task<bool> SendAsync(string phoneNumber, string code, string purpose, CancellationToken cancellationToken = default)
    {
        var settings = await Load();
        if (settings is not { SmsEnabled: true } || string.IsNullOrWhiteSpace(settings.SmsApiKeyProtected)) return false;
        string apiKey;
        try { apiKey = protector.Unprotect(settings.SmsApiKeyProtected); }
        catch (Exception ex) when (ex is CryptographicException or ArgumentException) { return false; }
        if (!string.Equals(settings.SmsProvider, "Kavenegar", StringComparison.OrdinalIgnoreCase)) return false;

        var client = clients.CreateClient("integrations");
        var endpoint = $"https://api.kavenegar.com/v1/{Uri.EscapeDataString(apiKey)}/sms/send.json";
        using var response = await client.PostAsync(endpoint, new FormUrlEncodedContent(new Dictionary<string, string?>
        {
            ["receptor"] = phoneNumber,
            ["sender"] = settings.SmsSender,
            ["message"] = $"کد تأیید فروشگاه: {code}"
        }), cancellationToken);
        if (!response.IsSuccessStatusCode) return false;
        using var json = await JsonDocument.ParseAsync(await response.Content.ReadAsStreamAsync(cancellationToken), cancellationToken: cancellationToken);
        return json.RootElement.TryGetProperty("return", out var result) && result.TryGetProperty("status", out var status) && status.GetInt32() == 200;
    }

    private Task<SiteIntegrationSettings?> Load() => db.SiteIntegrationSettings.AsNoTracking().SingleOrDefaultAsync(x => x.Id == 1);
}
