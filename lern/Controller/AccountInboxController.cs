using System.Security.Claims;
using Entities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace lern.Controller;

[ApiController]
[Authorize]
[Route("api/account/inbox")]
[ResponseCache(Location = ResponseCacheLocation.None, NoStore = true)]
public sealed class AccountInboxController(ShopDbContext db) : ControllerBase
{
    [HttpGet]
    public async Task<IActionResult> Get()
    {
        if (!int.TryParse(User.FindFirstValue(ClaimTypes.NameIdentifier), out var userId)) return Unauthorized();
        var items = await db.SiteInboxItems.AsNoTracking()
            .Where(x => x.RecipientUserId == null || x.RecipientUserId == userId)
            .OrderByDescending(x => x.CreatedAt).ThenByDescending(x => x.Id).Take(40)
            .Select(x => new { x.Id, x.Kind, x.Title, x.Body, x.CreatedAt }).ToListAsync();
        return Ok(items);
    }
}
