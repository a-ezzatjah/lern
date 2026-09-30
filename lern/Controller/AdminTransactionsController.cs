using Entities;
using lern.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace lern.Controller;

[Authorize(Roles = "Admin")]
[Route("Admin/Transactions")]
[ResponseCache(Location = ResponseCacheLocation.None, NoStore = true)]
public sealed class AdminTransactionsController(ShopDbContext db) : Microsoft.AspNetCore.Mvc.Controller
{
    [HttpGet("")]
    public async Task<IActionResult> Index(string? search, PaymentStatus? status, string? type,
        DateTime? from, DateTime? to, int page = 1, int pageSize = 10)
    {
        search = search?.Trim();
        if (status.HasValue && !Enum.IsDefined(status.Value)) status = null;
        if (type is not ("sale" or "refund")) type = null;
        pageSize = new[] { 10, 25, 50 }.Contains(pageSize) ? pageSize : 10;
        if (from.HasValue && to.HasValue && from.Value.Date > to.Value.Date)
            ModelState.AddModelError("from", "تاریخ شروع باید قبل از تاریخ پایان باشد.");

        var query = db.PaymentTransactions.AsNoTracking().AsQueryable();
        if (!string.IsNullOrWhiteSpace(search))
        {
            var digits = string.Concat(search.TrimStart('#').Select(c => char.GetNumericValue(c) is >= 0 and <= 9
                ? (char)('0' + (int)char.GetNumericValue(c)) : c));
            int.TryParse(digits, out var id);
            query = query.Where(x => x.Id == id || x.OrderId == id || (x.Reference != null && x.Reference.Contains(search))
                || (x.Order.CustomerFirstName + " " + x.Order.CustomerLastName).Contains(search)
                || x.Order.CustomerPhone.Contains(search));
        }
        if (status.HasValue) query = query.Where(x => x.Status == status.Value);
        if (type == "refund") query = query.Where(x => x.Status == PaymentStatus.Refunded);
        if (type == "sale") query = query.Where(x => x.Status != PaymentStatus.Refunded);
        if (from.HasValue)
        {
            var start = TimeZoneInfo.ConvertTimeToUtc(DateTime.SpecifyKind(from.Value.Date, DateTimeKind.Unspecified), AdminOrderDisplay.TimeZone);
            query = query.Where(x => x.CreatedAt >= start);
        }
        if (to.HasValue && to.Value.Date < DateTime.MaxValue.Date)
        {
            var end = TimeZoneInfo.ConvertTimeToUtc(DateTime.SpecifyKind(to.Value.Date.AddDays(1), DateTimeKind.Unspecified), AdminOrderDisplay.TimeZone);
            query = query.Where(x => x.CreatedAt < end);
        }

        var count = await query.CountAsync();
        page = Math.Clamp(page, 1, Math.Max(1, (int)Math.Ceiling(count / (double)pageSize)));
        var model = new AdminTransactionListViewModel
        {
            Search = search, Status = status, Type = type, From = from, To = to,
            Page = page, PageSize = pageSize, TotalCount = count,
            FailedCount = await query.CountAsync(x => x.Status == PaymentStatus.Failed),
            SuccessfulAmount = await query.Where(x => x.Status == PaymentStatus.Successful).SumAsync(x => (decimal?)x.Amount) ?? 0,
            AverageAmount = await query.AverageAsync(x => (decimal?)x.Amount) ?? 0,
            Transactions = await query.Include(x => x.Order).OrderByDescending(x => x.CreatedAt).ThenByDescending(x => x.Id)
                .Skip((page - 1) * pageSize).Take(pageSize).ToListAsync()
        };
        return View("~/Views/Admin/Transactions/Index.cshtml", model);
    }
}
