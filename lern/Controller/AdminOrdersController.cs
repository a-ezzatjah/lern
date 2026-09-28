using System.Globalization;
using Entities;
using lern.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace lern.Controller;

[Authorize(Roles = "Admin")]
[Route("Admin/Orders")]
[ResponseCache(Location = ResponseCacheLocation.None, NoStore = true)]
public sealed class AdminOrdersController(ShopDbContext db) : Microsoft.AspNetCore.Mvc.Controller
{
    [HttpGet("")]
    public async Task<IActionResult> Index(string? search, OrderStatus? status, string? period,
        DateTime? from, DateTime? to, int page = 1)
    {
        var query = db.Orders.AsNoTracking().AsQueryable();
        search = search?.Trim();
        if (!string.IsNullOrEmpty(search))
        {
            var normalized = string.Concat(search.TrimStart('#').Select(c => char.GetNumericValue(c) is >= 0 and <= 9 ? (char)('0' + (int)char.GetNumericValue(c)) : c));
            int.TryParse(normalized, out var id);
            query = query.Where(x => x.Id == id || (x.CustomerFirstName + " " + x.CustomerLastName).Contains(search)
                || x.CustomerPhone.Contains(search));
        }
        if (status.HasValue && Enum.IsDefined(status.Value)) query = query.Where(x => x.Status == status.Value);
        else status = null;

        var today = AdminOrderDisplay.LocalTime(DateTime.UtcNow).Date;
        var calendar = new PersianCalendar();
        DateTime? start = period switch
        {
            "today" => today,
            "week" => today.AddDays(-(((int)today.DayOfWeek + 1) % 7)),
            "month" => today.AddDays(1 - calendar.GetDayOfMonth(today)),
            "year" => calendar.ToDateTime(calendar.GetYear(today), 1, 1, 0, 0, 0, 0),
            "custom" => from?.Date,
            _ => null
        };
        if (period is not ("today" or "week" or "month" or "year" or "custom")) period = null;
        if (period == "custom" && from.HasValue && to.HasValue && from.Value.Date > to.Value.Date)
            ModelState.AddModelError("from", "تاریخ شروع باید قبل از تاریخ پایان باشد.");
        if (start.HasValue)
        {
            var utcStart = TimeZoneInfo.ConvertTimeToUtc(DateTime.SpecifyKind(start.Value, DateTimeKind.Unspecified), AdminOrderDisplay.TimeZone);
            query = query.Where(x => x.CreatedAt >= utcStart);
        }
        if (period == "custom" && to.HasValue && to.Value.Date < DateTime.MaxValue.Date)
        {
            var utcEnd = TimeZoneInfo.ConvertTimeToUtc(DateTime.SpecifyKind(to.Value.Date.AddDays(1), DateTimeKind.Unspecified), AdminOrderDisplay.TimeZone);
            query = query.Where(x => x.CreatedAt < utcEnd);
        }
        const int pageSize = 10;
        var count = await query.CountAsync();
        page = Math.Clamp(page, 1, Math.Max(1, (int)Math.Ceiling(count / (double)pageSize)));
        var orders = await query.Include(x => x.Transactions).OrderByDescending(x => x.CreatedAt).ThenByDescending(x => x.Id)
            .Skip((page - 1) * pageSize).Take(pageSize).ToListAsync();
        return View("~/Views/Admin/Orders/Index.cshtml", new AdminOrderListViewModel
        {
            Orders = orders, Search = search, Status = status, Period = period, From = from, To = to,
            Page = page, PageSize = pageSize, TotalCount = count
        });
    }

    [HttpGet("{id:int}")]
    public async Task<IActionResult> Details(int id)
    {
        var order = await db.Orders.AsNoTracking()
            .Include(x => x.Items).ThenInclude(x => x.Product).ThenInclude(x => x.ProductImages)
            .Include(x => x.Items).ThenInclude(x => x.ProductVariant)
            .Include(x => x.Transactions)
            .AsSplitQuery().SingleOrDefaultAsync(x => x.Id == id);
        if (order is null) return NotFound();

        var address = await db.Addresses.AsNoTracking().Where(x => x.CustomerKey == order.CustomerKey)
            .OrderByDescending(x => x.IsDefault).ThenByDescending(x => x.UpdatedAt).FirstOrDefaultAsync();
        string? email = null;
        if (order.CustomerKey.StartsWith("user-", StringComparison.Ordinal) &&
            int.TryParse(order.CustomerKey.AsSpan("user-".Length), out var userId))
            email = await db.Users.AsNoTracking().Where(x => x.Id == userId).Select(x => x.Email).SingleOrDefaultAsync();

        return View("~/Views/Admin/Orders/Details.cshtml", new AdminOrderDetailsViewModel
        {
            Order = order, CurrentAddress = address, CustomerEmail = email
        });
    }
}
