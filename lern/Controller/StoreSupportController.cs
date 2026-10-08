using Entities;
using lern.Infrastructure;
using lern.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;

namespace lern.Controller;

public sealed class StoreSupportController(ShopDbContext db) : Microsoft.AspNetCore.Mvc.Controller
{
    [HttpGet("/about")]
    public IActionResult About() => Page("/about", "About");

    [HttpGet("/contact")]
    public IActionResult Contact() => Page("/contact", "Contact");

    [HttpGet("/faq")]
    public IActionResult Faq() => Page("/faq", "Faq");

    [HttpGet("/complaints")]
    [ResponseCache(Location = ResponseCacheLocation.None, NoStore = true)]
    public IActionResult Complaints() => Page("/complaints", "Complaints", new ComplaintViewModel());

    [HttpPost("/complaints")]
    [ValidateAntiForgeryToken]
    [EnableRateLimiting("complaints")]
    [RequestSizeLimit(32_768)]
    [ResponseCache(Location = ResponseCacheLocation.None, NoStore = true)]
    public async Task<IActionResult> SubmitComplaint(ComplaintViewModel model, CancellationToken cancellationToken)
    {
        if (!string.IsNullOrWhiteSpace(model.Website)) return BadRequest();
        if (!ModelState.IsValid) return Page("/complaints", "Complaints", model);
        var complaint = new StoreComplaint
        {
            FullName = model.FullName.Trim(), Phone = model.Phone, OrderNumber = model.OrderNumber,
            Subject = model.Subject!.Value, Message = model.Message.Trim()
        };
        db.StoreComplaints.Add(complaint);
        try { await db.SaveChangesAsync(cancellationToken); }
        catch (Microsoft.EntityFrameworkCore.DbUpdateException)
        {
            db.Entry(complaint).State = Microsoft.EntityFrameworkCore.EntityState.Detached;
            ModelState.AddModelError("", "ثبت درخواست انجام نشد. لطفاً دوباره تلاش کنید یا با فروشگاه تماس بگیرید.");
            return Page("/complaints", "Complaints", model);
        }
        TempData["ComplaintReference"] = complaint.Reference;
        return RedirectToAction(nameof(Complaints));
    }

    private IActionResult Page(string path, string view, object? model = null)
    {
        var page = StoreSupportContent.Pages.Single(x => x.Path == path);
        ViewData["Title"] = page.MetaTitle;
        ViewData["MetaDescription"] = page.MetaDescription;
        ViewData["MetaKeywords"] = "خرازی کوهستانی، لوازم خیاطی، خرج کار لباس، یراق‌آلات، بازار بزرگ تهران";
        ViewData["SupportPageTitle"] = page.Title;
        return View(view, model);
    }
}
