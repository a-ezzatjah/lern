using Entities;
using Microsoft.EntityFrameworkCore;
using ServiceContract.Interfaces;
namespace lern.Infrastructure;

public sealed record PaymentFlowResult(bool Succeeded, string Message, int? OrderId = null, string? RedirectUrl = null);

public sealed class OrderPayments(ShopDbContext db, IOrderPaymentGateway gateway)
{
    public async Task<PaymentFlowResult> StartAsync(int orderId, string customerKey, string callbackUrl)
    {
        if (!gateway.IsConfigured) return new(false, "درگاه پرداخت هنوز فعال نشده است؛ هیچ مبلغی دریافت نشده است.");
        await using var tx = db.Database.CurrentTransaction is null ? await db.Database.BeginTransactionAsync() : null;
        await PurchaseLimitPolicy.LockCustomerAsync(db, customerKey);
        var order = await db.Orders.SingleOrDefaultAsync(x => x.Id == orderId && x.CustomerKey == customerKey);
        if (order is null) return new(false, "سفارش پیدا نشد.");
        if (order.Status != OrderStatus.Pending || order.ReservationExpiresAt <= DateTime.UtcNow)
            return new(false, "این سفارش قابل پرداخت نیست یا مهلت رزرو آن تمام شده است.", order.Id);
        var existing = await db.PaymentTransactions.AnyAsync(x => x.OrderId == orderId && x.Status == PaymentStatus.Pending && x.Authority != null);
        if (existing) return new(false, "یک پرداخت برای این سفارش در جریان است؛ ابتدا نتیجهٔ همان پرداخت را بررسی کنید.", order.Id);
        var result = await gateway.StartAsync(orderId, order.Total, callbackUrl, order.CustomerPhone);
        if (!result.Succeeded || string.IsNullOrWhiteSpace(result.Authority) || result.Authority.Length > 200 ||
            !Uri.TryCreate(result.RedirectUrl, UriKind.Absolute, out var url) || url.Scheme != Uri.UriSchemeHttps)
            return new(false, result.Error ?? "ارتباط با درگاه انجام نشد.", order.Id);
        db.PaymentTransactions.Add(new PaymentTransaction { OrderId = orderId, Amount = order.Total, Gateway = gateway.Name, Authority = result.Authority, Status = PaymentStatus.Pending });
        await db.SaveChangesAsync(); if (tx is not null) await tx.CommitAsync();
        return new(true, "انتقال به درگاه پرداخت", order.Id, result.RedirectUrl);
    }

    public async Task<PaymentFlowResult> VerifyAsync(string authority)
    {
        if (!gateway.IsConfigured || string.IsNullOrWhiteSpace(authority) || authority.Length > 200)
            return new(false, "پرداخت قابل تأیید نیست.");
        var payment = await db.PaymentTransactions.AsNoTracking().Include(x => x.Order)
            .SingleOrDefaultAsync(x => x.Gateway == gateway.Name && x.Authority == authority);
        if (payment is null) return new(false, "تراکنش پیدا نشد.");
        if (payment.Status == PaymentStatus.Successful) return new(true, "پرداخت قبلاً تأیید شده است.", payment.OrderId);
        // No callback success flag is trusted; verification uses the amount saved in the database.
        var verified = await gateway.VerifyAsync(authority, payment.Amount);
        if (!verified.Succeeded || string.IsNullOrWhiteSpace(verified.Reference))
        {
            if (verified.DefinitiveFailure)
                await db.PaymentTransactions.Where(x => x.Id == payment.Id && x.Status == PaymentStatus.Pending)
                    .ExecuteUpdateAsync(setters => setters.SetProperty(x => x.Status, PaymentStatus.Failed));
            return new(false, verified.Error ?? "پرداخت تأیید نشده است؛ در صورت کسر وجه، نتیجه را پیگیری کنید.", payment.OrderId);
        }
        await using var tx = db.Database.CurrentTransaction is null ? await db.Database.BeginTransactionAsync() : null;
        await PurchaseLimitPolicy.LockCustomerAsync(db, payment.Order.CustomerKey);
        var current = await db.PaymentTransactions.Include(x => x.Order).ThenInclude(x => x.Items).SingleAsync(x => x.Id == payment.Id);
        if (current.Status == PaymentStatus.Successful) return new(true, "پرداخت قبلاً تأیید شده است.", current.OrderId);
        var order = current.Order;
        if (await db.PaymentTransactions.AnyAsync(x => x.OrderId == order.Id && x.Id != current.Id && x.Status == PaymentStatus.Successful))
        {
            current.Status = PaymentStatus.Successful; current.Reference = verified.Reference; current.VerifiedAt = DateTime.UtcNow;
            order.Status = OrderStatus.PaymentReview;
        }
        else if (order.Status == OrderStatus.Pending && order.ReservationExpiresAt > DateTime.UtcNow)
        {
            await db.Database.CurrentTransaction!.CreateSavepointAsync("inventory");
            var fulfilled = true;
            foreach (var item in order.Items.OrderBy(x => x.ProductVariantId))
            {
                var changed = await db.ProductVariants.Where(x => x.Id == item.ProductVariantId && x.StockQuantity >= item.Quantity && x.ReservedQuantity >= item.Quantity)
                    .ExecuteUpdateAsync(setters => setters.SetProperty(x => x.StockQuantity, x => x.StockQuantity - item.Quantity)
                        .SetProperty(x => x.ReservedQuantity, x => x.ReservedQuantity - item.Quantity));
                if (changed != 1) { fulfilled = false; break; }
            }
            if (!fulfilled)
            {
                await db.Database.CurrentTransaction!.RollbackToSavepointAsync("inventory");
                order.Status = OrderStatus.PaymentReview;
            }
            else order.Status = OrderStatus.Paid;
            current.Status = PaymentStatus.Successful; current.Reference = verified.Reference; current.VerifiedAt = DateTime.UtcNow;
        }
        else
        {
            // A late confirmed payment must remain visible for reconciliation; never silently discard real money.
            if (order.Status == OrderStatus.Pending) await new OrderReservations(db).ReleaseAsync(order);
            order.Status = OrderStatus.PaymentReview;
            current.Status = PaymentStatus.Successful; current.Reference = verified.Reference; current.VerifiedAt = DateTime.UtcNow;
        }
        order.PaymentReference = verified.Reference;
        await db.SaveChangesAsync(); if (tx is not null) await tx.CommitAsync();
        return new(true, order.Status == OrderStatus.Paid ? "پرداخت تأیید شد." : "پرداخت دریافت شد و سفارش برای بررسی به مدیر ارجاع شد.", order.Id);
    }
}
