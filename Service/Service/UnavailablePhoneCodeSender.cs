using ServiceContract.Interfaces;
namespace Service.Service;

public sealed class UnavailablePhoneCodeSender : IPhoneCodeSender
{
    public bool IsConfigured => false;
    public Task<bool> SendAsync(string phoneNumber, string code, string purpose, CancellationToken cancellationToken = default)
        => Task.FromResult(false);
}
