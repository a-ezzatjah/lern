using System.Security.Claims;
using Entities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ServiceContract.DTO.DtoProductComment;

namespace lern.Controller;

[ApiController]
[Route("api/products/{productId:int}/rating")]
public sealed class ProductRatingController : ControllerBase
{
    private readonly ShopDbContext _db;

    public ProductRatingController(ShopDbContext db) => _db = db;

    [Authorize]
    [HttpPut]
    public async Task<IActionResult> Save(int productId, [FromBody] SaveProductRatingRequest request)
    {
        if (request.Score is < 1 or > 5)
            return BadRequest(new { message = "امتیاز باید بین ۱ تا ۵ باشد." });

        if (!await _db.Products.AnyAsync(x => x.Id == productId))
            return NotFound();

        var customerKey = $"user-{User.FindFirstValue(ClaimTypes.NameIdentifier)}";
        var rating = await _db.ProductRatings.SingleOrDefaultAsync(x => x.ProductId == productId && x.CustomerKey == customerKey);
        if (rating is null)
        {
            _db.ProductRatings.Add(new ProductRating { ProductId = productId, CustomerKey = customerKey, Score = request.Score });
        }
        else
        {
            rating.Score = request.Score;
            rating.UpdatedAt = DateTime.UtcNow;
        }

        await _db.SaveChangesAsync();
        return Ok(new { score = request.Score });
    }

    [Authorize]
    [HttpPost("/api/products/{productId:int}/comments")]
    public async Task<IActionResult> SaveComment(int productId, [FromBody] SaveProductCommentRequest request)
    {
        var body = request.Body?.Trim();
        var title = request.Title?.Trim();

        if (string.IsNullOrWhiteSpace(body) || body.Length > 2000)
            return BadRequest(new { message = "متن دیدگاه باید بین ۱ تا ۲۰۰۰ کاراکتر باشد." });
        if (title?.Length > 150)
            return BadRequest(new { message = "عنوان دیدگاه حداکثر ۱۵۰ کاراکتر است." });
        if (request.Score is < 1 or > 5)
            return BadRequest(new { message = "امتیاز باید بین ۱ تا ۵ باشد." });

        var customerKey = GetCustomerKey();
        if (customerKey is null)
            return Unauthorized();

        var productExists = await _db.Products.AnyAsync(x => x.Id == productId && x.IsActive);
        if (!productExists)
            return NotFound();

        var userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
        var user = await _db.Users.AsNoTracking().SingleOrDefaultAsync(x => x.Id == userId && x.IsActive);
        if (user is null)
            return Unauthorized();

        var now = DateTime.UtcNow;
        var comment = new ProductComment
        {
            ProductId = productId,
            CustomerKey = customerKey,
            AuthorName = $"{user.FirstName} {user.LastName}".Trim(),
            Title = string.IsNullOrWhiteSpace(title) ? null : title,
            Body = body,
            IsApproved = true,
            CreatedAt = now,
            UpdatedAt = now
        };
        _db.ProductComments.Add(comment);

        var rating = await _db.ProductRatings
            .SingleOrDefaultAsync(x => x.ProductId == productId && x.CustomerKey == customerKey);
        if (rating is null)
            _db.ProductRatings.Add(new ProductRating { ProductId = productId, CustomerKey = customerKey, Score = request.Score, CreatedAt = now, UpdatedAt = now });
        else
        {
            rating.Score = request.Score;
            rating.UpdatedAt = now;
        }

        await _db.SaveChangesAsync();
        return Ok(new { comment.Id, comment.IsApproved, message = "دیدگاه شما با موفقیت ثبت شد." });
    }

    [AllowAnonymous]
    [HttpGet("/api/products/{productId:int}/comments")]
    public async Task<ActionResult<ProductCommentListResponse>> GetComments(int productId, [FromQuery] int page = 1, [FromQuery] int pageSize = 10)
    {
        page = Math.Max(page, 1);
        pageSize = Math.Clamp(pageSize, 1, 50);

        if (!await _db.Products.AsNoTracking().AnyAsync(x => x.Id == productId && x.IsActive))
            return NotFound();

        var query = _db.ProductComments.AsNoTracking()
            .Where(x => x.ProductId == productId && x.IsApproved)
            .OrderByDescending(x => x.CreatedAt);

        var totalCount = await query.CountAsync();
        var comments = await query.Skip((page - 1) * pageSize).Take(pageSize)
            .Select(x => new ProductCommentDto
            {
                Id = x.Id,
                AuthorName = x.AuthorName,
                Title = x.Title,
                Body = x.Body,
                CreatedAt = x.CreatedAt
            }).ToListAsync();
        var ratingSummary = await _db.ProductRatings.AsNoTracking().Where(x => x.ProductId == productId)
            .GroupBy(x => x.ProductId)
            .Select(x => new { AverageScore = x.Average(r => (decimal)r.Score), RatingCount = x.Count() })
            .SingleOrDefaultAsync();

        return Ok(new ProductCommentListResponse(
            comments,
            totalCount,
            page,
            pageSize,
            ratingSummary?.AverageScore ?? 0,
            ratingSummary?.RatingCount ?? 0));
    }

    [Authorize(Roles = "Admin")]
    [HttpPut("/api/product-comments/{id:int}/approval")]
    public async Task<IActionResult> SetApproval(int id, [FromBody] ModerateProductCommentRequest request)
    {
        var comment = await _db.ProductComments.SingleOrDefaultAsync(x => x.Id == id);
        if (comment is null)
            return NotFound();

        comment.IsApproved = request.IsApproved;
        comment.UpdatedAt = DateTime.UtcNow;
        await _db.SaveChangesAsync();
        return Ok(new { comment.Id, comment.IsApproved });
    }

    private string? GetCustomerKey()
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        return int.TryParse(userId, out _) ? $"user-{userId}" : null;
    }
}

public record SaveProductRatingRequest(int Score);
public record SaveProductCommentRequest(string? Title, string? Body, int Score);
public record ModerateProductCommentRequest(bool IsApproved);
public record ProductCommentListResponse(
    IReadOnlyList<ProductCommentDto> Comments,
    int TotalCount,
    int Page,
    int PageSize,
    decimal AverageScore,
    int RatingCount);
