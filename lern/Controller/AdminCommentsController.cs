using Entities;
using lern.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace lern.Controller;

[Authorize(Roles = "Admin")]
[Route("Admin/Comments")]
[ResponseCache(Location = ResponseCacheLocation.None, NoStore = true)]
public sealed class AdminCommentsController(ShopDbContext db) : Microsoft.AspNetCore.Mvc.Controller
{
    [HttpGet("")]
    public async Task<IActionResult> Index(string? status = "pending", string? q = null, int page = 1, bool partial = false)
    {
        status = status is "approved" or "blocked" or "all" ? status : "pending";
        q = q?.Trim();
        if (q?.Length > 100) q = q[..100];
        var query = db.ProductComments.AsNoTracking();
        var pending = await query.CountAsync(x => !x.IsApproved && !x.IsBlocked);
        var approved = await query.CountAsync(x => x.IsApproved && !x.IsBlocked);
        var blocked = await query.CountAsync(x => x.IsBlocked);
        query = status switch
        {
            "approved" => query.Where(x => x.IsApproved && !x.IsBlocked),
            "blocked" => query.Where(x => x.IsBlocked),
            "pending" => query.Where(x => !x.IsApproved && !x.IsBlocked),
            _ => query
        };
        if (!string.IsNullOrEmpty(q)) query = query.Where(x => x.AuthorName.Contains(q) || x.Body.Contains(q) || (x.Title != null && x.Title.Contains(q)) || x.Product.Name.Contains(q));
        var count = await query.CountAsync();
        page = Math.Clamp(page, 1, Math.Max(1, (count + AdminCommentsViewModel.PageSize - 1) / AdminCommentsViewModel.PageSize));
        var model = new AdminCommentsViewModel
        {
            Comments = await query.Include(x => x.Product).OrderByDescending(x => x.CreatedAt).ThenByDescending(x => x.Id).Skip((page - 1) * AdminCommentsViewModel.PageSize).Take(AdminCommentsViewModel.PageSize).ToListAsync(),
            Query = q, Status = status, PendingCount = pending, ApprovedCount = approved, BlockedCount = blocked, TotalCount = count, Page = page
        };
        return partial ? PartialView("_Batch", model) : View(model);
    }

    [HttpPost("{id:int}/moderate")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Moderate(int id, string decision, DateTime updatedAt, string? status = "pending", string? q = null, int page = 1)
    {
        if (decision is not ("approved" or "blocked" or "pending")) return BadRequest();
        // Approve only the exact text version displayed to the administrator.
        var now = DateTime.UtcNow;
        var changed = await db.ProductComments.Where(x => x.Id == id && x.UpdatedAt == updatedAt)
            .ExecuteUpdateAsync(setters => setters
                .SetProperty(x => x.IsApproved, decision == "approved")
                .SetProperty(x => x.IsBlocked, decision == "blocked")
                .SetProperty(x => x.UpdatedAt, now));
        if (changed == 0)
        {
            if (!await db.ProductComments.AnyAsync(x => x.Id == id)) return NotFound();
            TempData["CommentMessage"] = "این نظر تغییر کرده است؛ متن جدید را دوباره بررسی کنید.";
            return RedirectToAction(nameof(Index), new { status, q, page });
        }
        TempData["CommentMessage"] = decision switch
        {
            "approved" => "نظر تأیید و منتشر شد.",
            "blocked" => "نظر مسدود شد و در سایت نمایش داده نمی‌شود.",
            _ => "نظر به صف بررسی بازگشت."
        };
        return RedirectToAction(nameof(Index), new { status, q, page });
    }
}
