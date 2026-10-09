using Entities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace lern.Controller;

[Authorize(Roles = "Admin")]
[Route("Admin/Complaints")]
[Route("Admin/Settings/complaints")]
[ResponseCache(Location = ResponseCacheLocation.None, NoStore = true)]
public sealed class AdminComplaintsController(ShopDbContext db) : Microsoft.AspNetCore.Mvc.Controller
{
    [HttpGet("")]
    public async Task<IActionResult> Index(int page = 1, bool? reviewed = null)
    {
        var query = db.StoreComplaints.AsNoTracking();
        if (reviewed.HasValue) query = query.Where(x => x.IsReviewed == reviewed.Value);
        var count = await query.CountAsync();
        page = Math.Clamp(page, 1, Math.Max(1, (int)Math.Ceiling(count / 20d)));
        ViewData["Page"] = page;
        ViewData["TotalPages"] = Math.Max(1, (int)Math.Ceiling(count / 20d));
        ViewData["Reviewed"] = reviewed;
        ViewData["Count"] = count;
        return View(await query.OrderByDescending(x => x.CreatedAt).ThenByDescending(x => x.Id).Skip((page - 1) * 20).Take(20).ToListAsync());
    }

    [HttpPost("{id:int}/review")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Review(int id)
    {
        var complaint = await db.StoreComplaints.FindAsync(id);
        if (complaint is null) return NotFound();
        complaint.IsReviewed = true;
        await db.SaveChangesAsync();
        return RedirectToAction(nameof(Index));
    }
}
