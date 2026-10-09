using System.Reflection;
using System.Security.Claims;
using System.Text.Json;
using Entities;
using lern.Controller;
using lern.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Microsoft.EntityFrameworkCore;

void Check(bool value, string label) { if (!value) throw new Exception(label); Console.WriteLine("PASS " + label); }
using var settings = JsonDocument.Parse(await File.ReadAllTextAsync("lern/appsettings.json"));
var connection = settings.RootElement.GetProperty("ConnectionStrings").GetProperty("DefaultConnectionstring").GetString();
await using var db = new ShopDbContext(new DbContextOptionsBuilder<ShopDbContext>().UseSqlServer(connection).Options);
Check(!(await db.Database.GetPendingMigrationsAsync()).Any(), "moderation migration applied");
Check(typeof(AdminCommentsController).GetCustomAttribute<AuthorizeAttribute>()?.Roles == "Admin", "management restricted to Admin");
Check(typeof(AdminCommentsController).GetMethod("Moderate")!.GetCustomAttribute<ValidateAntiForgeryTokenAttribute>() != null, "moderation requires antiforgery token");
await using var transaction = await db.Database.BeginTransactionAsync();
var user = await db.Users.AsNoTracking().FirstAsync(x => x.IsActive);
var productId = await db.Products.Where(x => x.IsActive).Select(x => x.Id).FirstAsync();
var context = new DefaultHttpContext { User = new ClaimsPrincipal(new ClaimsIdentity(new[] { new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()) }, "smoke")) };
var api = new ProductRatingController(db) { ControllerContext = new ControllerContext { HttpContext = context } };
var account = new AccountController(db, null!, null!) { ControllerContext = new ControllerContext { HttpContext = context } };
var admin = new AdminCommentsController(db) { ControllerContext = new ControllerContext { HttpContext = context }, TempData = new TempDataDictionary(context, new MemoryTempData()) };
var marker = "Moderation smoke " + Guid.NewGuid();
Check(await api.SaveComment(productId, new SaveProductCommentRequest("Smoke", marker, 4)) is OkObjectResult, "customer submits review");
var comment = await db.ProductComments.SingleAsync(x => x.Body == marker);
async Task Refresh() => await db.Entry(comment).ReloadAsync();
async Task<bool> Published() { var response = await api.GetComments(productId, 1, 50); return ((ProductCommentListResponse)((OkObjectResult)response.Result!).Value!).Comments.Any(x => x.Id == comment.Id); }
async Task<bool> AccountHas(string status) { var model = (AccountCommentsViewModel)((ViewResult)await account.Comments(status, null, null, null)).Model!; return model.Comments.Any(x => x.Id == comment.Id); }
Check(!comment.IsApproved && !comment.IsBlocked && !await Published(), "new review stays pending and private");
Check(await AccountHas("pending") && !await AccountHas("blocked"), "account pending filter excludes blocked");
var pendingModel = (AdminCommentsViewModel)((ViewResult)await admin.Index("pending", marker)).Model!;
Check(pendingModel.Comments.Any(x => x.Id == comment.Id), "pending queue and search find review");
var originalVersion = comment.UpdatedAt;
await admin.Moderate(comment.Id, "approved", originalVersion); await Refresh();
Check(comment.IsApproved && !comment.IsBlocked && await Published() && await AccountHas("approved"), "approved review becomes public and green status");
await admin.Moderate(comment.Id, "blocked", comment.UpdatedAt); await Refresh();
Check(!comment.IsApproved && comment.IsBlocked && !await Published(), "blocking removes review from public list");
Check(await AccountHas("blocked") && !await AccountHas("pending"), "account red status differs from pending");
await admin.Moderate(comment.Id, "approved", originalVersion); await Refresh();
Check(comment.IsBlocked, "stale admin form cannot approve changed review");
Check(await admin.Moderate(comment.Id, "invalid", comment.UpdatedAt) is BadRequestResult, "invalid moderation decision rejected");
await api.UpdateComment(comment.Id, new UpdateProductCommentRequest("Edited", marker + " API", null)); await Refresh();
Check(!comment.IsApproved && !comment.IsBlocked && !await Published(), "API edit resubmits blocked review for moderation");
await admin.Moderate(comment.Id, "approved", comment.UpdatedAt); await Refresh();
await account.EditComment(comment.Id, "Account edit", marker + " account", null); await Refresh();
Check(!comment.IsApproved && !comment.IsBlocked && !await Published(), "account edit removes previous approval");
await api.SetApproval(comment.Id, new ModerateProductCommentRequest(false)); await Refresh();
Check(comment.IsBlocked && !comment.IsApproved && !await Published(), "legacy admin rejection records blocked state");
context.User = new ClaimsPrincipal(new ClaimsIdentity(new[] { new Claim(ClaimTypes.NameIdentifier, "-1") }, "smoke"));
Check(await api.UpdateComment(comment.Id, new UpdateProductCommentRequest(null, "Attempt", null)) is NotFoundResult, "other customer cannot edit review");
Check(await account.EditComment(comment.Id, null, "Attempt", null) is NotFoundResult, "account ownership enforced");
var batchMarker = "Batch " + Guid.NewGuid();
var batchTime = DateTime.UtcNow;
for (var i = 0; i < 23; i++) db.ProductComments.Add(new ProductComment { ProductId = productId, CustomerKey = "user-" + user.Id, AuthorName = "Batch customer", Body = batchMarker, CreatedAt = batchTime, UpdatedAt = batchTime });
await db.SaveChangesAsync();
var first = (AdminCommentsViewModel)((ViewResult)await admin.Index("pending", batchMarker, 1)).Model!;
var secondResult = (PartialViewResult)await admin.Index("pending", batchMarker, 2, true);
var second = (AdminCommentsViewModel)secondResult.Model!;
var third = (AdminCommentsViewModel)((PartialViewResult)await admin.Index("pending", batchMarker, 3, true)).Model!;
Check(first.Comments.Count == 10 && second.Comments.Count == 10 && third.Comments.Count == 3, "progressive pages contain 10, 10 and 3 comments");
Check(first.TotalPages == 3 && secondResult.ViewName == "_Batch", "partial response uses batch view and correct page count");
Check(first.Comments.Concat(second.Comments).Concat(third.Comments).Select(x => x.Id).Distinct().Count() == 23, "tied timestamps do not duplicate or skip comments");
Check(!((AdminCommentsViewModel)((PartialViewResult)await admin.Index("blocked", batchMarker, 1, true)).Model!).Comments.Any(), "progressive requests preserve status filter");
await transaction.RollbackAsync();
Console.WriteLine("All test data and rating changes rolled back.");

sealed class MemoryTempData : ITempDataProvider
{
    public IDictionary<string, object> LoadTempData(HttpContext context) => new Dictionary<string, object>();
    public void SaveTempData(HttpContext context, IDictionary<string, object> values) { }
}
