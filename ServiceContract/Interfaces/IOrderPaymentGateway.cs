namespace ServiceContract.Interfaces;

// Provider adapters must verify the stored authority and amount with the gateway server.
// Browser callback parameters are never proof of payment.
public interface IOrderPaymentGateway
{
    string Name { get; }
    bool IsConfigured { get; }
    Task<GatewayStartResult> StartAsync(int orderId, decimal amountInToman, string callbackUrl, string phone, CancellationToken cancellationToken = default);
    Task<GatewayVerifyResult> VerifyAsync(string authority, decimal amountInToman, CancellationToken cancellationToken = default);
}
public sealed record GatewayStartResult(bool Succeeded, string? Authority = null, string? RedirectUrl = null, string? Error = null);
public sealed record GatewayVerifyResult(bool Succeeded, string? Reference = null, string? Error = null, bool DefinitiveFailure = false);
