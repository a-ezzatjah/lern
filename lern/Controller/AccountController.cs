using System.Security.Claims;
using Entities;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using lern.Models;
using ServiceContract.Interfaces;

namespace lern.Controller;

public sealed class AccountController : Microsoft.AspNetCore.Mvc.Controller
{
    private readonly ShopDbContext _db;
    private readonly IProductService _products;
    private readonly IUserAuthService _accountUsers;

    public AccountController(ShopDbContext db, IProductService products, IUserAuthService accountUsers)
    {
        _db = db;
        _products = products;
        _accountUsers = accountUsers;
    }
    [AllowAnonymous]
    [HttpGet("/login")]
    public IActionResult Login(string? returnUrl)
    {
        ViewData["ReturnUrl"] = Url.IsLocalUrl(returnUrl) ? returnUrl : "/account";
        return View();
    }

    [AllowAnonymous]
    [HttpGet("/account/access-denied")]
    [HttpGet("/Account/AccessDenied")]
    [ResponseCache(Location = ResponseCacheLocation.None, NoStore = true)]
    public IActionResult AccessDenied(string? returnUrl)
    {
        ViewData["ReturnUrl"] = Url.IsLocalUrl(returnUrl) ? returnUrl : "/Admin/Orders";
        Response.StatusCode = StatusCodes.Status403Forbidden;
        return View();
    }

    [AllowAnonymous]
    [HttpGet("/register")]
    public IActionResult Register() => View();

    [Authorize]
    [HttpGet("/account")]
    public async Task<IActionResult> Index()
    {
        var customerKey = $"user-{User.FindFirstValue(ClaimTypes.NameIdentifier)}";
        var userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
        var now = DateTime.UtcNow;
        var yearAgo = now.AddYears(-1);
        var activityDates = new[]
        {
            await _db.ProductViewHistories.AsNoTracking().Where(x => x.CustomerKey == customerKey && x.LastViewedAt >= yearAgo).Select(x => x.LastViewedAt).ToListAsync(),
            await _db.CartItems.AsNoTracking().Where(x => x.CustomerKey == customerKey && x.UpdatedAt >= yearAgo).Select(x => x.UpdatedAt).ToListAsync(),
            await _db.Orders.AsNoTracking().Where(x => x.CustomerKey == customerKey && x.CreatedAt >= yearAgo).Select(x => x.CreatedAt).ToListAsync(),
            await _db.ProductFavorites.AsNoTracking().Where(x => x.UserId == userId && x.CreatedAt >= yearAgo).Select(x => x.CreatedAt).ToListAsync(),
            await _db.ProductComments.AsNoTracking().Where(x => x.CustomerKey == customerKey && x.CreatedAt >= yearAgo).Select(x => x.CreatedAt).ToListAsync()
        };
        var viewedProductIds = await _db.ProductViewHistories.AsNoTracking()
            .Where(x => x.CustomerKey == customerKey)
            .OrderByDescending(x => x.LastViewedAt)
            .Select(x => x.ProductId)
            .Take(8)
            .ToListAsync();
        var productCards = await _products.GetProductCardsAsync();
        var cardsById = productCards.ToDictionary(x => x.Id);
        var recentlyViewedProducts = viewedProductIds
            .Where(cardsById.ContainsKey)
            .Select(id => cardsById[id])
            .ToList();

        return View(new AccountIndexViewModel
        {
            RecentlyViewedProducts = recentlyViewedProducts,
            ActivityCounts = new Dictionary<string, int[]>
            {
                ["week"] = activityDates.Select(dates => dates.Count(date => date >= now.AddDays(-7))).ToArray(),
                ["month"] = activityDates.Select(dates => dates.Count(date => date >= now.AddMonths(-1))).ToArray(),
                ["year"] = activityDates.Select(dates => dates.Count).ToArray()
            },
            RecentTransactions = await _db.PaymentTransactions.AsNoTracking().Include(x => x.Order)
                .Where(x => x.Order.CustomerKey == customerKey &&
                    (x.Status == PaymentStatus.Successful || x.Status == PaymentStatus.Failed))
                .OrderByDescending(x => x.CreatedAt).ThenByDescending(x => x.Id).Take(4).ToListAsync(),
            RecentOrders = await _db.Orders.AsNoTracking().Include(x => x.Items)
                .Where(x => x.CustomerKey == customerKey).OrderByDescending(x => x.CreatedAt).Take(3).ToListAsync(),
            ActiveOrderCount = await _db.Orders.CountAsync(x => x.CustomerKey == customerKey &&
                (x.Status == OrderStatus.Pending || x.Status == OrderStatus.Paid || x.Status == OrderStatus.Processing || x.Status == OrderStatus.Shipped)),
            ShippedOrderCount = await _db.Orders.CountAsync(x => x.CustomerKey == customerKey && x.Status == OrderStatus.Shipped),
            FavoriteCount = await _db.ProductFavorites.CountAsync(x => x.UserId == int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!) && x.Product.IsActive)
        });
    }

    [Authorize]
    [HttpGet("/account/discounts")]
    public async Task<IActionResult> Discounts()
    {
        if (!int.TryParse(User.FindFirstValue(ClaimTypes.NameIdentifier), out var userId)) return Forbid();
        return View(new AccountDiscountsViewModel
        {
            Coupons = await _db.DiscountCoupons.AsNoTracking()
                .Where(x => x.RecipientUserId == null || x.RecipientUserId == userId)
                .OrderByDescending(x => x.CreatedAt).ThenByDescending(x => x.Id).ToListAsync(),
            UsedCouponIds = (await _db.CouponRedemptions.AsNoTracking().Where(x => x.UserId == userId)
                .Select(x => x.CouponId).ToListAsync()).ToHashSet()
        });
    }

    [Authorize]
    [HttpGet("/account/change-password")]
    [ResponseCache(Location = ResponseCacheLocation.None, NoStore = true)]
    public IActionResult ChangePassword() => View();

    [Authorize]
    [HttpPost("/account/change-password")]
    [ValidateAntiForgeryToken]
    [ResponseCache(Location = ResponseCacheLocation.None, NoStore = true)]
    public async Task<IActionResult> ChangePassword(ChangePasswordViewModel model)
    {
        if (!ModelState.IsValid) return View(model);
        if (!int.TryParse(User.FindFirstValue(ClaimTypes.NameIdentifier), out var userId)) return Forbid();
        var result = await _accountUsers.ChangePasswordAsync(userId, model.CurrentPassword, model.NewPassword);
        if (!result.Succeeded)
        {
            ModelState.AddModelError(string.Empty, result.ErrorMessage ?? "تغییر رمز عبور انجام نشد.");
            return View(model);
        }
        TempData["PasswordChanged"] = "رمز عبور شما با موفقیت تغییر کرد.";
        return RedirectToAction(nameof(ChangePassword));
    }

    [Authorize]
    [HttpGet("/account/profile")]
    public IActionResult Profile() => View();

    [Authorize]
    [HttpPost("/account/logout")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Logout()
    {
        await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
        return Redirect("/");
    }

    [Authorize]
    [HttpGet("/account/orders")]
    [ResponseCache(Location = ResponseCacheLocation.None, NoStore = true)]
    public async Task<IActionResult> Orders(OrderStatus? status, string? period, string? amount, string? search, int page = 1)
    {
        var customerKey = $"user-{User.FindFirstValue(ClaimTypes.NameIdentifier)}";
        var orders = _db.Orders.AsNoTracking().Where(x => x.CustomerKey == customerKey);
        search = search?.Trim().TrimStart('#');
        if (!string.IsNullOrEmpty(search))
        {
            if (int.TryParse(search, out var orderId)) orders = orders.Where(x => x.Id == orderId);
            else orders = orders.Where(x => false);
        }
        if (status.HasValue && Enum.IsDefined(status.Value)) orders = orders.Where(x => x.Status == status.Value);
        else status = null;

        var since = period switch
        {
            "7days" => DateTime.UtcNow.AddDays(-7),
            "30days" => DateTime.UtcNow.AddDays(-30),
            "3months" => DateTime.UtcNow.AddMonths(-3),
            "year" => DateTime.UtcNow.AddYears(-1),
            _ => (DateTime?)null
        };
        if (since.HasValue) orders = orders.Where(x => x.CreatedAt >= since.Value);
        else period = null;

        orders = amount switch
        {
            "less500" => orders.Where(x => x.Total < 500_000),
            "500-1000" => orders.Where(x => x.Total >= 500_000 && x.Total < 1_000_000),
            "1000-5000" => orders.Where(x => x.Total >= 1_000_000 && x.Total < 5_000_000),
            "more5000" => orders.Where(x => x.Total >= 5_000_000),
            _ => orders
        };
        if (amount is not ("less500" or "500-1000" or "1000-5000" or "more5000")) amount = null;

        const int pageSize = 10;
        var totalCount = await orders.CountAsync();
        page = Math.Clamp(page, 1, Math.Max(1, (totalCount + pageSize - 1) / pageSize));
        return View(new AccountOrdersViewModel
        {
            Orders = await orders.Include(x => x.Transactions).OrderByDescending(x => x.CreatedAt).ThenByDescending(x => x.Id)
                .Skip((page - 1) * pageSize).Take(pageSize).ToListAsync(),
            Status = status, Period = period, Amount = amount, Search = search, Page = page, TotalCount = totalCount
        });
    }

    [Authorize]
    [HttpGet("/account/orders/{id:int}")]
    [ResponseCache(Location = ResponseCacheLocation.None, NoStore = true)]
    public async Task<IActionResult> OrderDetails(int id)
    {
        var customerKey = $"user-{User.FindFirstValue(ClaimTypes.NameIdentifier)}";
        var order = await _db.Orders.AsNoTracking().Include(x => x.Items)
            .SingleOrDefaultAsync(x => x.Id == id && x.CustomerKey == customerKey);
        return order is null ? NotFound() : View(order);
    }

    [Authorize]
    [HttpGet("/account/addresses")]
    [ResponseCache(Location = ResponseCacheLocation.None, NoStore = true)]
    public async Task<IActionResult> Addresses()
    {
        var key = $"user-{User.FindFirstValue(ClaimTypes.NameIdentifier)}";
        return View(await _db.Addresses.AsNoTracking().Where(x => x.CustomerKey == key)
            .OrderByDescending(x => x.IsDefault).ThenByDescending(x => x.UpdatedAt).ToListAsync());
    }

    [Authorize]
    [ValidateAntiForgeryToken]
    [HttpPost("/account/addresses/{id:int}/delete")]
    public async Task<IActionResult> DeleteAddress(int id)
    {
        var key = $"user-{User.FindFirstValue(ClaimTypes.NameIdentifier)}";
        var address = await _db.Addresses.SingleOrDefaultAsync(x => x.Id == id && x.CustomerKey == key);
        if (address is null) return NotFound();
        _db.Addresses.Remove(address);
        await _db.SaveChangesAsync();
        return RedirectToAction(nameof(Addresses));
    }

    [Authorize]
    [ValidateAntiForgeryToken]
    [HttpPost("/account/addresses/{id:int}/default")]
    public async Task<IActionResult> SetDefaultAddress(int id)
    {
        var key = $"user-{User.FindFirstValue(ClaimTypes.NameIdentifier)}";
        var address = await _db.Addresses.SingleOrDefaultAsync(x => x.Id == id && x.CustomerKey == key);
        if (address is null) return NotFound();
        await _db.Addresses.Where(x => x.CustomerKey == key && x.Id != id)
            .ExecuteUpdateAsync(setters => setters.SetProperty(x => x.IsDefault, false));
        address.IsDefault = true;
        address.UpdatedAt = DateTime.UtcNow;
        await _db.SaveChangesAsync();
        return RedirectToAction(nameof(Addresses));
    }

    [Authorize]
    [HttpGet("/account/comments")]
    [ResponseCache(Location = ResponseCacheLocation.None, NoStore = true)]
    public async Task<IActionResult> Comments(string? status, int? rating, string? period, string? sort, int page = 1)
    {
        var key = $"user-{User.FindFirstValue(ClaimTypes.NameIdentifier)}";
        var query = _db.ProductComments.AsNoTracking().Where(x => x.CustomerKey == key);
        if (status == "approved") query = query.Where(x => x.IsApproved);
        else if (status == "pending") query = query.Where(x => !x.IsApproved);
        else status = null;
        if (rating is >= 1 and <= 5)
            query = query.Where(x => _db.ProductRatings.Any(r => r.ProductId == x.ProductId && r.CustomerKey == key && r.Score == rating));
        else rating = null;
        var since = period switch
        {
            "today" => DateTime.UtcNow.AddDays(-1),
            "week" => DateTime.UtcNow.AddDays(-7),
            "month" => DateTime.UtcNow.AddMonths(-1),
            "year" => DateTime.UtcNow.AddYears(-1),
            _ => (DateTime?)null
        };
        if (since.HasValue) query = query.Where(x => x.CreatedAt >= since.Value);
        else period = null;
        sort = sort is "oldest" or "highest" or "lowest" ? sort : "newest";
        query = sort switch
        {
            "oldest" => query.OrderBy(x => x.CreatedAt).ThenBy(x => x.Id),
            "highest" => query.OrderByDescending(x => _db.ProductRatings.Where(r => r.ProductId == x.ProductId && r.CustomerKey == key).Select(r => (int?)r.Score).FirstOrDefault()).ThenByDescending(x => x.CreatedAt),
            "lowest" => query.OrderBy(x => _db.ProductRatings.Where(r => r.ProductId == x.ProductId && r.CustomerKey == key).Select(r => (int?)r.Score).FirstOrDefault()).ThenByDescending(x => x.CreatedAt),
            _ => query.OrderByDescending(x => x.CreatedAt).ThenByDescending(x => x.Id)
        };
        const int pageSize = 10;
        var total = await query.CountAsync();
        page = Math.Clamp(page, 1, Math.Max(1, (total + pageSize - 1) / pageSize));
        return View(new AccountCommentsViewModel
        {
            Comments = await query.Skip((page - 1) * pageSize).Take(pageSize)
                .Select(x => new AccountCommentRow(x.Id, x.ProductId, x.Product.Name, x.AuthorName,
                    x.Title, x.Body, x.IsApproved, x.CreatedAt,
                    _db.ProductRatings.Where(r => r.ProductId == x.ProductId && r.CustomerKey == key)
                        .Select(r => (int?)r.Score).FirstOrDefault())).ToListAsync(),
            Status = status, Rating = rating, Period = period, Sort = sort, Page = page, TotalCount = total
        });
    }

    [Authorize]
    [HttpGet("/account/comments/edit")]
    [ResponseCache(Location = ResponseCacheLocation.None, NoStore = true)]
    public async Task<IActionResult> EditComments()
    {
        var key = $"user-{User.FindFirstValue(ClaimTypes.NameIdentifier)}";
        var comments = await _db.ProductComments.AsNoTracking().Where(x => x.CustomerKey == key)
            .OrderByDescending(x => x.CreatedAt).Select(x => new AccountCommentRow(x.Id, x.ProductId,
                x.Product.Name, x.AuthorName, x.Title, x.Body, x.IsApproved, x.CreatedAt, null)).ToListAsync();
        return View(comments);
    }

    [Authorize]
    [ValidateAntiForgeryToken]
    [HttpPost("/account/comments/{id:int}/delete")]
    public async Task<IActionResult> DeleteComment(int id)
    {
        var key = $"user-{User.FindFirstValue(ClaimTypes.NameIdentifier)}";
        var comment = await _db.ProductComments.SingleOrDefaultAsync(x => x.Id == id && x.CustomerKey == key);
        if (comment is null) return NotFound();

        _db.ProductComments.Remove(comment);
        await _db.SaveChangesAsync();
        return RedirectToAction(nameof(EditComments));
    }

    [Authorize]
    [HttpGet("/account/comments/{id:int}/edit")]
    [ResponseCache(Location = ResponseCacheLocation.None, NoStore = true)]
    public async Task<IActionResult> EditComment(int id)
    {
        var key = $"user-{User.FindFirstValue(ClaimTypes.NameIdentifier)}";
        var comment = await _db.ProductComments.AsNoTracking().Where(x => x.Id == id && x.CustomerKey == key)
            .Select(x => new AccountCommentEditViewModel
            {
                Id = x.Id, ProductId = x.ProductId, ProductName = x.Product.Name,
                Title = x.Title, Body = x.Body, IsApproved = x.IsApproved,
                Score = _db.ProductRatings.Where(r => r.ProductId == x.ProductId && r.CustomerKey == key)
                    .Select(r => (int?)r.Score).FirstOrDefault()
            }).SingleOrDefaultAsync();
        return comment is null ? NotFound() : View(comment);
    }

    [Authorize]
    [ValidateAntiForgeryToken]
    [HttpPost("/account/comments/{id:int}/edit")]
    public async Task<IActionResult> EditComment(int id, string? title, string? body, int? score)
    {
        var key = $"user-{User.FindFirstValue(ClaimTypes.NameIdentifier)}";
        var comment = await _db.ProductComments.Include(x => x.Product)
            .SingleOrDefaultAsync(x => x.Id == id && x.CustomerKey == key);
        if (comment is null) return NotFound();
        var rating = await _db.ProductRatings.SingleOrDefaultAsync(x => x.ProductId == comment.ProductId && x.CustomerKey == key);
        var model = new AccountCommentEditViewModel
        {
            Id = id, ProductId = comment.ProductId, ProductName = comment.Product.Name,
            Title = title?.Trim(), Body = body?.Trim() ?? "", Score = score, IsApproved = comment.IsApproved
        };
        if (model.Body.Length is < 1 or > 2000 || model.Title?.Length > 150 || score is < 1 or > 5)
        {
            model.Error = "متن نظر باید ۱ تا ۲۰۰۰ کاراکتر، عنوان حداکثر ۱۵۰ کاراکتر و امتیاز بین ۱ تا ۵ باشد.";
            return View(model);
        }
        comment.Title = string.IsNullOrWhiteSpace(model.Title) ? null : model.Title;
        comment.Body = model.Body;
        comment.UpdatedAt = DateTime.UtcNow;
        if (score.HasValue)
        {
            if (rating is null)
                _db.ProductRatings.Add(new ProductRating { ProductId = comment.ProductId, CustomerKey = key, Score = score.Value });
            else
            {
                rating.Score = score.Value;
                rating.UpdatedAt = DateTime.UtcNow;
            }
        }
        await _db.SaveChangesAsync();
        return RedirectToAction(nameof(Comments));
    }
}
