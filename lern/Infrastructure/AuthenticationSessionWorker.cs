using Entities;
using Microsoft.EntityFrameworkCore;

namespace lern.Infrastructure;

public sealed class AuthenticationSessionWorker(IServiceScopeFactory scopes, TimeProvider clock, ILogger<AuthenticationSessionWorker> logger) : BackgroundService
{
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                await using var scope = scopes.CreateAsyncScope();
                var db = scope.ServiceProvider.GetRequiredService<ShopDbContext>();
                await CleanupAsync(db, clock, stoppingToken);
            }
            catch (OperationCanceledException) when (stoppingToken.IsCancellationRequested) { break; }
            catch (Exception error) { logger.LogWarning(error, "Authentication session cleanup failed."); }
            try { await Task.Delay(TimeSpan.FromHours(1), clock, stoppingToken); }
            catch (OperationCanceledException) when (stoppingToken.IsCancellationRequested) { break; }
        }
    }

    public static Task<int> CleanupAsync(ShopDbContext db, TimeProvider clock, CancellationToken cancellationToken = default)
    {
        var now = clock.GetUtcNow().UtcDateTime;
        var idleCutoff = now.Subtract(SqlAuthenticationTicketStore.IdleTimeout);
        return db.AuthenticationSessions.Where(x => x.ExpiresAt <= now || x.LastSeenAt <= idleCutoff ||
            !x.User.IsActive || x.AuthenticationVersion != x.User.AuthenticationVersion || x.Role != x.User.Role)
            .ExecuteDeleteAsync(cancellationToken);
    }
}
