namespace ServiceContract.Interfaces;

public interface IPhoneCodeSender
{
    bool IsConfigured { get; }
    // Provider adapters must never log the code or credentials. Success means accepted for delivery.
    Task<bool> SendAsync(string phoneNumber, string code, string purpose, CancellationToken cancellationToken = default);
}
