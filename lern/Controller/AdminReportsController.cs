using System.Globalization;
using Entities;
using lern.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace lern.Controller;

[Authorize(Roles = "Admin")]
[Route("Admin/Reports")]
[ResponseCache(Location = ResponseCacheLocation.None, NoStore = true)]
public sealed class AdminReportsController(ShopDbContext db) : Microsoft.AspNetCore.Mvc.Controller
{
    private static readonly PersianCalendar Calendar = new();
    private static readonly TimeZoneInfo Tehran = AdminOrderDisplay.TimeZone;

    [HttpGet("")]
    public async Task<IActionResult> Index(DateTime? from, DateTime? to, int? categoryId, int page = 1)
    {
        var today = TimeZoneInfo.ConvertTimeFromUtc(DateTime.UtcNow, Tehran).Date;
        var startDate = from?.Date ?? today.AddDays(-2);
        var endDate = to?.Date ?? today;
        if (startDate > endDate || (endDate - startDate).TotalDays > 366)
        {
            ModelState.AddModelError("from", "بازه تاریخ باید مرتب و حداکثر ۳۶۷ روز باشد.");
            startDate = today.AddDays(-2);
            endDate = today;
        }
        var reportedTo = endDate > startDate.AddDays(49) ? startDate.AddDays(49) : endDate;
        var totalDays = (reportedTo - startDate).Days + 1;
        var totalPages = (totalDays + 9) / 10;
        page = Math.Clamp(page, 1, totalPages);
        var startUtc = TimeZoneInfo.ConvertTimeToUtc(DateTime.SpecifyKind(startDate, DateTimeKind.Unspecified), Tehran);
        var endUtc = TimeZoneInfo.ConvertTimeToUtc(DateTime.SpecifyKind(reportedTo.AddDays(1), DateTimeKind.Unspecified), Tehran);
        var categories = await db.Categories.AsNoTracking().OrderBy(x => x.Name)
            .Select(x => new ReportCategoryOption(x.Id, x.Name)).ToListAsync();
        if (categoryId.HasValue && categories.All(x => x.Id != categoryId)) categoryId = null;

        var orders = await db.Orders.AsNoTracking()
            .Where(x => x.CreatedAt >= startUtc && x.CreatedAt < endUtc &&
                x.Status != OrderStatus.Cancelled && x.Status != OrderStatus.Pending)
            .Select(x => new { x.Id, x.CreatedAt, x.Total, x.Subtotal }).ToListAsync();
        var orderIds = orders.Select(x => x.Id).ToArray();
        var items = await db.OrderItems.AsNoTracking().Where(x => orderIds.Contains(x.OrderId))
            .Select(x => new { x.OrderId, x.ProductId, x.LineTotal }).ToListAsync();
        var productIds = items.Select(x => x.ProductId).Distinct().ToArray();
        var links = await db.ProductCategories.AsNoTracking().Where(x => productIds.Contains(x.ProductId))
            .Select(x => new { x.ProductId, x.CategoryId }).ToListAsync();
        var categoryMap = links.GroupBy(x => x.ProductId).ToDictionary(x => x.Key, x => x.Select(y => y.CategoryId).Distinct().ToArray());
        var orderMap = orders.ToDictionary(x => x.Id);
        // Allocate each line to its categories evenly and apply the order-level discount proportionally.
        var amounts = items.SelectMany(item =>
        {
            var ids = categoryMap.GetValueOrDefault(item.ProductId) ?? [];
            if (ids.Length == 0) ids = [0];
            var order = orderMap[item.OrderId];
            var net = order.Subtotal > 0 ? item.LineTotal * order.Total / order.Subtotal : 0;
            return ids.Select(id => new { item.OrderId, CategoryId = id, Amount = net / ids.Length });
        }).Where(x => !categoryId.HasValue || x.CategoryId == categoryId.Value).ToList();
        var salesByOrder = categoryId.HasValue
            ? amounts.GroupBy(x => x.OrderId).ToDictionary(x => x.Key, x => x.Sum(y => y.Amount))
            : orders.ToDictionary(x => x.Id, x => x.Total);
        var selectedOrders = orders.Where(x => salesByOrder.ContainsKey(x.Id)).ToList();
        var users = await db.Users.AsNoTracking().Where(x => x.CreatedAt >= startUtc && x.CreatedAt < endUtc)
            .Select(x => x.CreatedAt).ToListAsync();
        var customersByDate = users.GroupBy(x => TimeZoneInfo.ConvertTimeFromUtc(DateTime.SpecifyKind(x, DateTimeKind.Utc), Tehran).Date)
            .ToDictionary(x => x.Key, x => x.Count());
        var orderDays = selectedOrders.GroupBy(x => TimeZoneInfo.ConvertTimeFromUtc(DateTime.SpecifyKind(x.CreatedAt, DateTimeKind.Utc), Tehran).Date)
            .ToDictionary(x => x.Key, x => x.ToList());
        var daily = Enumerable.Range(0, totalDays).Select(i =>
        {
            var day = startDate.AddDays(i);
            var dayOrders = orderDays.GetValueOrDefault(day);
            return new ReportPoint(day, dayOrders?.Count ?? 0, dayOrders?.Sum(x => salesByOrder[x.Id]) ?? 0,
                categoryId.HasValue ? 0 : customersByDate.GetValueOrDefault(day));
        }).ToList();
        var months = daily.GroupBy(x => (Calendar.GetYear(x.Date), Calendar.GetMonth(x.Date))).ToList();
        var distribution = amounts.GroupBy(x => x.CategoryId).OrderByDescending(x => x.Sum(y => y.Amount)).ToList();
        var names = categories.ToDictionary(x => x.Id, x => x.Name);
        var model = new AdminSalesReportViewModel
        {
            From = startDate, To = endDate, ReportedTo = reportedTo, IsTruncated = reportedTo < endDate,
            Page = page, TotalPages = totalPages, CategoryId = categoryId, Categories = categories,
            Sales = selectedOrders.Sum(x => salesByOrder[x.Id]), OrderCount = selectedOrders.Count,
            NewCustomers = categoryId.HasValue ? 0 : users.Count,
            Daily = daily.Skip((page - 1) * 10).Take(10).OrderByDescending(x => x.Date).ToList(),
            MonthLabels = months.Select(x => $"{x.Key.Item1:0000}/{x.Key.Item2:00}").ToArray(),
            MonthSales = months.Select(x => x.Sum(y => y.Sales)).ToArray(),
            CategoryLabels = distribution.Select(x => names.GetValueOrDefault(x.Key, "بدون دسته‌بندی")).ToArray(),
            CategorySales = distribution.Select(x => x.Sum(y => y.Amount)).ToArray()
        };
        return View("~/Views/Admin/Reports/Index.cshtml", model);
    }
}
