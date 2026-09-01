using Entities;
using lern.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using DTO;

namespace lern.Controller;

[Route("Admin")]
public class AdminController : Microsoft.AspNetCore.Mvc.Controller
{
    private readonly ShopDbContext _db;
    private readonly IWebHostEnvironment _environment;
    private readonly ILogger<AdminController> _logger;

    public AdminController(
        ShopDbContext db,
        IWebHostEnvironment environment,
        ILogger<AdminController> logger)
    {
        _db = db;
        _environment = environment;
        _logger = logger;
    }

    [HttpGet("")]
    public async Task<IActionResult> Index()
    {
        var today = DateTime.UtcNow.Date;
        var firstMonth = new DateTime(today.Year, today.Month, 1).AddMonths(-5);
        var products = await _db.Products.AsNoTracking().ToListAsync();
        var orderCount = 0;
        var salesTotal = 0m;
        var chartValues = new decimal[6];
        try
        {
            var paidOrders = await _db.Orders.AsNoTracking()
                .Where(x => x.Status != OrderStatus.Cancelled && x.CreatedAt.Date == today)
                .ToListAsync();
            orderCount = paidOrders.Count;
            salesTotal = paidOrders.Sum(x => x.Total);
            for (var i = 0; i < 6; i++)
            {
                var month = firstMonth.AddMonths(i);
                chartValues[i] = await _db.Orders.AsNoTracking()
                    .Where(x => x.Status != OrderStatus.Cancelled && x.CreatedAt.Year == month.Year && x.CreatedAt.Month == month.Month)
                    .Select(x => (decimal?)x.Total).SumAsync() ?? 0m;
            }
        }
        catch (Microsoft.Data.SqlClient.SqlException exception) when (exception.Number == 208)
        {
            _logger.LogWarning("Order tables are not available yet. Run 'dotnet ef database update'.");
        }
        return View(new AdminDashboardViewModel
        {
            ProductCount = products.Count,
            ActiveProductCount = products.Count(x => x.IsActive),
            CategoryCount = await _db.Categories.AsNoTracking().CountAsync(),
            AvailableStock = Math.Max(0, await _db.ProductVariants.AsNoTracking()
                .SumAsync(x => x.StockQuantity - x.ReservedQuantity)),
            OrderCount = orderCount,
            SalesTotal = salesTotal,
            ChartLabels = Enumerable.Range(0, 6).Select(i => firstMonth.AddMonths(i).ToString("yyyy/MM")).ToArray(),
            ChartValues = chartValues
        });
    }

    [HttpGet("Products/Create")]
    public IActionResult CreateProductPage()
    {
        var model = new AdminProductCreateViewModel
        {
            Variants = Enumerable.Range(0, 1)
                .Select(_ => new AdminVariantInputViewModel())
                .ToList()
        };

        return View("Products/Create", model);
    }

    [HttpPost("Products")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> CreateProduct(AdminProductCreateViewModel model)
    {
        model.Variants ??= new();
        var variants = model.Variants
            .Where(x => !string.IsNullOrWhiteSpace(x.SaleTitle) && x.Price > 0)
            .ToList();

        if (!ModelState.IsValid || variants.Count == 0)
        {
            if (variants.Count == 0)
                ModelState.AddModelError(string.Empty, "حداقل یک تنوع با عنوان فروش و قیمت بیشتر از صفر وارد کنید.");

            while (model.Variants.Count < 1)
                model.Variants.Add(new AdminVariantInputViewModel());

            if (IsAjaxRequest())
                return BadRequest(new
                {
                    success = false,
                    message = "اطلاعات محصول کامل نیست.",
                    errors = ModelState.Values
                        .SelectMany(x => x.Errors)
                        .Select(x => x.ErrorMessage)
                        .Where(x => !string.IsNullOrWhiteSpace(x))
                        .ToList()
                });

            return View("Products/Create", model);
        }

        if (await _db.Products.AnyAsync(x => x.Name == model.Name || x.Slug == model.Slug))
        {
            ModelState.AddModelError(string.Empty, "نام یا Slug این محصول قبلاً ثبت شده است.");

            if (IsAjaxRequest())
                return Conflict(new
                {
                    success = false,
                    message = "نام یا Slug این محصول قبلاً ثبت شده است."
                });

            return View("Products/Create", model);
        }

        var now = DateTime.UtcNow;
        foreach (var input in variants)
        {
            if (input.Image is null || input.Image.Length == 0)
                continue;

            var extension = Path.GetExtension(input.Image.FileName).ToLowerInvariant();
            if (!new[] { ".jpg", ".jpeg", ".png", ".webp" }.Contains(extension))
            {
                ModelState.AddModelError(string.Empty, "فرمت هر تصویر باید JPG، PNG یا WEBP باشد.");

                if (IsAjaxRequest())
                    return BadRequest(new { success = false, message = "فرمت تصویر مجاز نیست." });

                return View("Products/Create", model);
            }

            if (input.Image.Length > 5 * 1024 * 1024)
            {
                ModelState.AddModelError(string.Empty, "حجم هر تصویر نباید بیشتر از ۵ مگابایت باشد.");

                if (IsAjaxRequest())
                    return BadRequest(new { success = false, message = "حجم تصویر نباید بیشتر از ۵ مگابایت باشد." });

                return View("Products/Create", model);
            }
        }

        var product = new Product
        {
            Name = model.Name.Trim(),
            Slug = model.Slug.Trim(),
            ShortDescription = model.ShortDescription,
            IsActive = model.IsActive,
            DiscountValue = model.ProductDiscountValue,
            DiscountType = ToDiscountType(model.ProductDiscountType),
            DiscountStartAt = model.ProductDiscountValue > 0 ? now.AddDays(-1) : null,
            DiscountEndAt = model.ProductDiscountValue > 0 ? now.AddDays(30) : null,
            CreatedAt = now
        };
        var createdVariants = new List<(AdminVariantInputViewModel Input, ProductVariant Variant)>();

        foreach (var group in variants.GroupBy(x => new { x.SaleTitle, x.SaleType }))
        {
            var saleOption = new ProductSaleOption
            {
                Product = product,
                Title = group.Key.SaleTitle.Trim(),
                SaleType = (Entities.Enums.EnumSaleType)group.Key.SaleType,
                UnitName = group.Key.SaleType == 2 ? "متر" : "عدد",
                Step = 1
            };

            foreach (var input in group)
            {
                ProductSaleOptionColor? color = null;
                if (!string.IsNullOrWhiteSpace(input.Color))
                {
                    color = new ProductSaleOptionColor
                    {
                        Color = input.Color.Trim(),
                        HexCode = input.HexCode,
                        ProductSaleOption = saleOption
                    };
                }

                var variant = new ProductVariant
                {
                    ProductSaleOption = saleOption,
                    saleoptioncolor = color,
                    Sku = $"{product.Slug}-{Guid.NewGuid():N}"[..24],
                    Price = input.Price,
                    StockQuantity = Math.Max(input.StockQuantity, 0),
                    ReservedQuantity = 0,
                    DiscountValue = input.DiscountValue,
                    DisconType = ToDiscountType(input.DiscountType),
                    DiscountStartAt = input.DiscountValue > 0 ? now.AddDays(-1) : null,
                    DiscountEndAt = input.DiscountValue > 0 ? now.AddDays(30) : null
                };

                saleOption.ProductVariants.Add(variant);

                createdVariants.Add((input, variant));
            }

            product.SaleOptions.Add(saleOption);
        }

        try
        {
            _db.Products.Add(product);
            await _db.SaveChangesAsync();

            var hasPrimaryImage = false;
            foreach (var item in createdVariants)
            {
                if (item.Input.Image is null || item.Input.Image.Length == 0)
                    continue;

                var imageUrl = await SaveImageAsync(item.Input.Image);
                _db.ProductImages.Add(new ProductImage
                {
                    ProductId = product.Id,
                    VariantId = item.Variant.Id,
                    ImageUrl = imageUrl,
                    AltText = $"{product.Name} - {item.Input.Color ?? item.Variant.ProductSaleOption.Title}",
                    IsPrimary = !hasPrimaryImage,
                    SortOrder = hasPrimaryImage ? 1 : 0
                });
                hasPrimaryImage = true;
            }

            if (hasPrimaryImage)
                await _db.SaveChangesAsync();
        }
        catch (Exception exception)
        {
            _logger.LogError(exception, "خطا در ثبت محصول جدید با نام {ProductName}", product.Name);

            if (IsAjaxRequest())
                return StatusCode(500, new
                {
                    success = false,
                    message = "ثبت محصول انجام نشد. ساختار دیتابیس و اطلاعات واردشده را بررسی کن.",
                    detail = _environment.IsDevelopment()
                        ? exception.GetBaseException().Message
                        : null
                });

            throw;
        }

        TempData["Success"] = $"محصول «{product.Name}» با موفقیت ثبت شد.";

        if (IsAjaxRequest())
            return Json(new
            {
                success = true,
                message = $"محصول «{product.Name}» با موفقیت اضافه شد."
            });

        return RedirectToAction(nameof(Index));
    }

    private static DisconTypeEnum? ToDiscountType(int? value)
    {
        return value is 1 or 2 ? (DisconTypeEnum)value.Value : null;
    }

    private async Task<string> SaveImageAsync(IFormFile image)
    {
        var uploadDirectory = Path.Combine(
            _environment.WebRootPath,
            "uploads",
            "products");

        Directory.CreateDirectory(uploadDirectory);

        var extension = Path.GetExtension(image.FileName).ToLowerInvariant();
        var fileName = $"{Guid.NewGuid():N}{extension}";
        var filePath = Path.Combine(uploadDirectory, fileName);

        await using var stream = new FileStream(filePath, FileMode.CreateNew);
        await image.CopyToAsync(stream);
        return $"/uploads/products/{fileName}";
    }

    private bool IsAjaxRequest()
    {
        return Request.Headers.XRequestedWith == "XMLHttpRequest" ||
               Request.Headers.Accept.ToString().Contains("application/json", StringComparison.OrdinalIgnoreCase);
    }
}
