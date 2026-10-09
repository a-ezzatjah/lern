using ServiceContract.Interfaces;
namespace Service.Service;

public sealed class UnavailablePaymentGateway : IOrderPaymentGateway
{
    public string Name => "disabled";
    public bool IsConfigured => false;
    public Task<GatewayStartResult> StartAsync(int orderId, decimal amountInToman, string callbackUrl, string phone, CancellationToken cancellationToken = default)
        => Task.FromResult(new GatewayStartResult(false, Error: "پرداخت آنلاین هنوز فعال نشده است؛ مبلغی دریافت نمی‌شود."));
    public Task<GatewayVerifyResult> VerifyAsync(string authority, decimal amountInToman, CancellationToken cancellationToken = default)
        => Task.FromResult(new GatewayVerifyResult(false, Error: "درگاه پرداخت فعال نیست."));
}
