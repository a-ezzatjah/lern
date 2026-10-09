using Entities;
using Microsoft.EntityFrameworkCore;
namespace lern.Infrastructure;

public sealed class OrderReservations(ShopDbContext db)
{
    public async Task<int> ExpireAsync(string? customerKey = null)
    {
        var now = DateTime.UtcNow;
        var query = db.Orders.AsNoTracking().Where(x => x.Status == OrderStatus.Pending && x.ReservationExpiresAt != null && x.ReservationExpiresAt <= now);
        if (customerKey is not null) query = query.Where(x => x.CustomerKey == customerKey);
        var candidates = await query.OrderBy(x => x.Id).Select(x => new { x.Id, x.CustomerKey }).Take(100).ToListAsync();
        var expired = 0;
        foreach (var candidate in candidates)
        {
            await using var tx = db.Database.CurrentTransaction is null ? await db.Database.BeginTransactionAsync() : null;
            await PurchaseLimitPolicy.LockCustomerAsync(db, candidate.CustomerKey);
            var order = await db.Orders.Include(x => x.Items).SingleAsync(x => x.Id == candidate.Id);
            if (order.Status != OrderStatus.Pending || order.ReservationExpiresAt > now) continue;
            await ReleaseAsync(order);
            order.Status = OrderStatus.Cancelled;
            var redemptions = await db.CouponRedemptions.Where(x => x.OrderId == order.Id).ToListAsync();
            db.CouponRedemptions.RemoveRange(redemptions);
            await db.SaveChangesAsync();
            if (tx is not null) await tx.CommitAsync();
            expired++;
        }
        return expired;
    }
    public async Task ReleaseAsync(Order order)
    {
        foreach (var item in order.Items.OrderBy(x => x.ProductVariantId))
        {
            var changed = await db.ProductVariants.Where(x => x.Id == item.ProductVariantId && x.ReservedQuantity >= item.Quantity)
                .ExecuteUpdateAsync(setters => setters.SetProperty(x => x.ReservedQuantity, x => x.ReservedQuantity - item.Quantity));
            if (changed != 1) throw new InvalidOperationException("موجودی رزروشدهٔ سفارش نیاز به بررسی دارد.");
        }
    }
}

public sealed class OrderReservationWorker(IServiceScopeFactory scopes, ILogger<OrderReservationWorker> logger) : BackgroundService
{
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        using var timer = new PeriodicTimer(TimeSpan.FromMinutes(2));
        while (await timer.WaitForNextTickAsync(stoppingToken))
        {
            try { await using var scope = scopes.CreateAsyncScope(); await scope.ServiceProvider.GetRequiredService<OrderReservations>().ExpireAsync(); }
            catch (Exception error) { logger.LogError(error, "آزادسازی رزرو سفارش‌های منقضی انجام نشد."); }
        }
    }
}
