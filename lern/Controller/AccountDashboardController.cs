using System.Security.Claims;
using Entities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace lern.Controller;

[ApiController]
[Authorize]
[Route("api/account/dashboard")]
public sealed class AccountDashboardController : ControllerBase
{
    private readonly ShopDbContext _db;

    public AccountDashboardController(ShopDbContext db) => _db = db;

    [HttpGet]
    public async Task<IActionResult> Get()
    {
        var customerKey = $"user-{User.FindFirstValue(ClaimTypes.NameIdentifier)}";
        var purchaseTotal = await _db.Orders.AsNoTracking()
            .Where(x => x.CustomerKey == customerKey && (x.Status == OrderStatus.Paid || x.Status == OrderStatus.Processing || x.Status == OrderStatus.Shipped || x.Status == OrderStatus.Completed))
            .SumAsync(x => (decimal?)x.Total) ?? 0m;
        var averageScore = await _db.ProductRatings.AsNoTracking()
            .Where(x => x.CustomerKey == customerKey)
            .AverageAsync(x => (double?)x.Score) ?? 0d;

        return Ok(new { purchaseTotal, ratingOutOf100 = Math.Round(averageScore * 20, 1) });
    }
}
