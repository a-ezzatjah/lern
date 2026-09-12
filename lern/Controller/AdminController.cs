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

    [HttpGet("Products")]
    public async Task<IActionResult> Products(string? search, int page = 1)
    {
        var query = _db.Products.AsNoTracking()
            .Include(x => x.ProductImages)
            .Include(x => x.ProductCategories).ThenInclude(x => x.Category)
            .Include(x => x.SaleOptions).ThenInclude(x => x.ProductVariants)
            .Include(x => x.SaleOptions).ThenInclude(x => x.SaleOptionColors)
            .AsQueryable();

        if (!string.IsNullOrWhiteSpace(search))
            query = query.Where(x => x.Name.Contains(search) || x.Slug.Contains(search));

        var totalCount = await query.CountAsync();
        var pageSize = 10;
        page = Math.Clamp(page, 1, Math.Max(1, (int)Math.Ceiling(totalCount / (double)pageSize)));
        var products = await query.OrderByDescending(x => x.CreatedAt).Skip((page - 1) * pageSize).Take(pageSize).ToListAsync();
        return View("Products/Index", new AdminProductListViewModel { Search = search, Products = products, Page = page, PageSize = pageSize, TotalCount = totalCount });
    }

    [HttpGet("Categories")]
    public async Task<IActionResult> Categories(string? search)
    {
        var query = _db.Categories.AsNoTracking().Include(x => x.Parent).AsQueryable();
        if (!string.IsNullOrWhiteSpace(search))
            query = query.Where(x => x.Name.Contains(search) || x.Slug.Contains(search) || (x.Parent != null && x.Parent.Name.Contains(search)));

        var categories = await query.OrderBy(x => x.ParentId).ThenBy(x => x.SortOrder).ThenBy(x => x.Name).ToListAsync();
        var parents = await _db.Categories.AsNoTracking().OrderBy(x => x.Name).ToListAsync();
        return View("Categories/Index", new AdminCategoryListViewModel { Search = search, Categories = categories, ParentOptions = parents });
    }

    [HttpPost("Categories")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> CreateCategory(string name, string slug, int? parentId, int? sortOrder)
    {
        var error = await ValidateCategoryAsync(name, slug, parentId);
        if (error is not null) return BadRequest(new { success = false, message = error });

        _db.Categories.Add(new Category { Name = name.Trim(), Slug = slug.Trim(), ParentId = parentId, SortOrder = sortOrder ?? 0 });
        await _db.SaveChangesAsync();
        return Json(new { success = true, message = "دسته‌بندی افزوده شد." });
    }

    [HttpPost("Categories/{id:int}/Edit")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> EditCategory(int id, string name, string slug, int? parentId, int? sortOrder)
    {
        var category = await _db.Categories.FindAsync(id);
        if (category is null) return NotFound(new { success = false, message = "دسته‌بندی پیدا نشد." });

        var error = await ValidateCategoryAsync(name, slug, parentId, id);
        if (error is not null) return BadRequest(new { success = false, message = error });

        category.Name = name.Trim();
        category.Slug = slug.Trim();
        category.ParentId = parentId;
        category.SortOrder = sortOrder ?? 0;
        await _db.SaveChangesAsync();
        return Json(new { success = true, message = "دسته‌بندی ویرایش شد." });
    }

    [HttpPost("Categories/{id:int}/Delete")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteCategory(int id)
    {
        var category = await _db.Categories.FindAsync(id);
        if (category is null) return NotFound(new { success = false, message = "دسته‌بندی پیدا نشد." });
        if (await _db.Categories.AnyAsync(x => x.ParentId == id))
            return Conflict(new { success = false, message = "این دسته زیرمجموعه دارد و قابل حذف نیست." });
        if (await _db.ProductCategories.AnyAsync(x => x.CategoryId == id))
            return Conflict(new { success = false, message = "این دسته به محصول متصل است و قابل حذف نیست." });

        _db.Categories.Remove(category);
        await _db.SaveChangesAsync();
        return Json(new { success = true, message = "دسته‌بندی حذف شد." });
    }

    [HttpGet("Products/Edit")]
    public async Task<IActionResult> EditProductBySlug(string slug)
    {
        var id = await _db.Products.Where(x => x.Slug == slug).Select(x => (int?)x.Id).FirstOrDefaultAsync();
        return id is null ? NotFound() : RedirectToAction(nameof(EditProductPage), new { id });
    }

    [HttpPost("Products/Delete")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteProduct(string slug)
    {
        var product = await _db.Products
            .Include(x => x.ProductImages)
            .Include(x => x.ProductCategories)
            .Include(x => x.SaleOptions).ThenInclude(x => x.ProductVariants)
            .FirstOrDefaultAsync(x => x.Slug == slug);
        if (product is null) return NotFound(new { success = false, message = "محصول پیدا نشد." });

        var variantIds = product.SaleOptions.SelectMany(x => x.ProductVariants).Select(x => x.Id).ToList();
        var images = product.ProductImages.Where(x => !x.VariantId.HasValue || variantIds.Contains(x.VariantId.Value));
        _db.ProductImages.RemoveRange(images);
        _db.ProductCategories.RemoveRange(product.ProductCategories);
        _db.ProductSaleOptions.RemoveRange(product.SaleOptions);
        _db.Products.Remove(product);
        try
        {
            await _db.SaveChangesAsync();
            return Json(new { success = true, message = "محصول حذف شد." });
        }
        catch (DbUpdateException)
        {
            return Conflict(new { success = false, message = "این محصول در سفارش‌ها استفاده شده و قابل حذف نیست." });
        }
    }

    [HttpGet("Products/{id:int}/Edit")]
    public async Task<IActionResult> EditProductPage(int id)
    {
        var product = await _db.Products
            .Include(x => x.ProductImages)
            .Include(x => x.SaleOptions).ThenInclude(x => x.ProductVariants).ThenInclude(x => x.saleoptioncolor)
            .FirstOrDefaultAsync(x => x.Id == id);
        if (product is null) return NotFound();

        var model = new AdminProductCreateViewModel
        {
            Id = product.Id,
            Name = product.Name,
            Slug = product.Slug,
            ShortDescription = product.ShortDescription,
            IsActive = product.IsActive,
            ProductDiscountValue = product.DiscountValue,
            ProductDiscountType = product.DiscountType is null ? null : (int)product.DiscountType.Value,
            ExistingPrimaryImageUrl = product.ProductImages.OrderByDescending(x => x.IsPrimary).ThenBy(x => x.SortOrder).FirstOrDefault()?.ImageUrl,
            Variants = product.SaleOptions.SelectMany(option => option.ProductVariants.Select(variant => new AdminVariantInputViewModel
            {
                SaleTitle = option.Title,
                SaleType = (int)option.SaleType,
                Color = variant.saleoptioncolor?.Color,
                HexCode = variant.saleoptioncolor?.HexCode,
                Price = variant.Price,
                StockQuantity = variant.StockQuantity,
                DiscountValue = variant.DiscountValue,
                DiscountType = variant.DisconType is null ? null : (int)variant.DisconType.Value
            })).ToList()
        };
        if (model.Variants.Count == 0) model.Variants.Add(new AdminVariantInputViewModel());
        return View("Products/Create", model);
    }

    [HttpPost("Products/{id:int}/Edit")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> EditProduct(int id, AdminProductCreateViewModel model)
    {
        model.Id = id;
        model.Variants ??= new();
        var variants = model.Variants.Where(x => !string.IsNullOrWhiteSpace(x.SaleTitle) && x.Price > 0).ToList();
        var product = await _db.Products
            .Include(x => x.ProductImages)
            .Include(x => x.SaleOptions).ThenInclude(x => x.ProductVariants)
            .FirstOrDefaultAsync(x => x.Id == id);
        if (product is null) return NotFound();

        if (!ModelState.IsValid || variants.Count == 0 || await _db.Products.AnyAsync(x => x.Id != id && (x.Name == model.Name || x.Slug == model.Slug)))
            return await InvalidProductForm(model, "اطلاعات محصول معتبر نیست یا نام/slug تکراری است.");

        if (!IsValidImage(model.PrimaryImage) || model.Variants.Any(x => !IsValidImage(x.Image)))
            return await InvalidProductForm(model, "فرمت تصویر باید JPG، PNG یا WEBP و حداکثر ۵ مگابایت باشد.");

        var now = DateTime.UtcNow;
        product.Name = model.Name.Trim(); product.Slug = model.Slug.Trim(); product.ShortDescription = model.ShortDescription;
        product.IsActive = model.IsActive; product.DiscountValue = model.ProductDiscountValue; product.DiscountType = ToDiscountType(model.ProductDiscountType);
        product.DiscountStartAt = model.ProductDiscountValue > 0 ? now.AddDays(-1) : null; product.DiscountEndAt = model.ProductDiscountValue > 0 ? now.AddDays(30) : null; product.UpdatedAt = now;

        var oldVariantIds = product.SaleOptions.SelectMany(x => x.ProductVariants).Select(x => x.Id).ToList();
        if (oldVariantIds.Count > 0) await _db.ProductImages.Where(x => x.VariantId.HasValue && oldVariantIds.Contains(x.VariantId.Value)).ExecuteDeleteAsync();
        _db.ProductSaleOptions.RemoveRange(product.SaleOptions);
        var createdVariants = AddVariants(product, variants, now);
        await _db.SaveChangesAsync();

        if (model.PrimaryImage is not null)
        {
            foreach (var image in product.ProductImages) image.IsPrimary = false;
            _db.ProductImages.Add(new ProductImage { ProductId = product.Id, ImageUrl = await SaveImageAsync(model.PrimaryImage), AltText = product.Name, IsPrimary = true, SortOrder = 0 });
        }
        await AddVariantImages(product, createdVariants);
        await _db.SaveChangesAsync();

        if (IsAjaxRequest()) return Json(new { success = true, message = "محصول با موفقیت ویرایش شد.", redirectUrl = Url.Action(nameof(Products)) });
        return RedirectToAction(nameof(Products));
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
        foreach (var input in variants.Append(new AdminVariantInputViewModel { Image = model.PrimaryImage }))
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

            if (model.PrimaryImage is not null && model.PrimaryImage.Length > 0)
            {
                _db.ProductImages.Add(new ProductImage
                {
                    ProductId = product.Id,
                    ImageUrl = await SaveImageAsync(model.PrimaryImage),
                    AltText = product.Name,
                    IsPrimary = true,
                    SortOrder = 0
                });
            }

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
                    IsPrimary = false,
                    SortOrder = 1
                });
            }

            if (model.PrimaryImage is not null || createdVariants.Any(x => x.Input.Image is not null && x.Input.Image.Length > 0))
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

    private async Task<string?> ValidateCategoryAsync(string? name, string? slug, int? parentId, int? editingId = null)
    {
        if (string.IsNullOrWhiteSpace(name) || string.IsNullOrWhiteSpace(slug))
            return "نام و slug دسته‌بندی الزامی است.";
        if (name.Trim().Length > 100 || slug.Trim().Length > 100)
            return "نام و slug نباید بیشتر از ۱۰۰ کاراکتر باشند.";
        if (await _db.Categories.AnyAsync(x => x.Id != editingId && (x.Name == name.Trim() || x.Slug == slug.Trim())))
            return "نام یا slug دسته‌بندی تکراری است.";
        if (parentId is null) return null;
        if (parentId == editingId) return "یک دسته نمی‌تواند والد خودش باشد.";
        if (!await _db.Categories.AnyAsync(x => x.Id == parentId)) return "دستهٔ والد معتبر نیست.";

        if (editingId is not null)
        {
            var descendantIds = new HashSet<int> { editingId.Value };
            var frontier = new List<int> { editingId.Value };
            while (frontier.Count > 0)
            {
                var children = await _db.Categories.Where(x => x.ParentId != null && frontier.Contains(x.ParentId.Value)).Select(x => x.Id).ToListAsync();
                frontier = children.Where(descendantIds.Add).ToList();
            }
            if (descendantIds.Contains(parentId.Value)) return "والد نمی‌تواند یکی از زیرمجموعه‌های همین دسته باشد.";
        }
        return null;
    }

    private static bool IsValidImage(IFormFile? image)
    {
        if (image is null || image.Length == 0) return true;
        var extension = Path.GetExtension(image.FileName).ToLowerInvariant();
        return image.Length <= 5 * 1024 * 1024 && new[] { ".jpg", ".jpeg", ".png", ".webp" }.Contains(extension);
    }

    private List<(AdminVariantInputViewModel Input, ProductVariant Variant)> AddVariants(Product product, List<AdminVariantInputViewModel> variants, DateTime now)
    {
        var created = new List<(AdminVariantInputViewModel Input, ProductVariant Variant)>();
        foreach (var group in variants.GroupBy(x => new { x.SaleTitle, x.SaleType }))
        {
            var option = new ProductSaleOption { Product = product, Title = group.Key.SaleTitle.Trim(), SaleType = (Entities.Enums.EnumSaleType)group.Key.SaleType, UnitName = group.Key.SaleType == 2 ? "متر" : "عدد", Step = 1 };
            foreach (var input in group)
            {
                ProductSaleOptionColor? color = null;
                if (!string.IsNullOrWhiteSpace(input.Color)) color = new ProductSaleOptionColor { Color = input.Color.Trim(), HexCode = input.HexCode, ProductSaleOption = option };
                var variant = new ProductVariant { ProductSaleOption = option, saleoptioncolor = color, Sku = $"{product.Slug}-{Guid.NewGuid():N}"[..24], Price = input.Price, StockQuantity = Math.Max(input.StockQuantity, 0), ReservedQuantity = 0, DiscountValue = input.DiscountValue, DisconType = ToDiscountType(input.DiscountType), DiscountStartAt = input.DiscountValue > 0 ? now.AddDays(-1) : null, DiscountEndAt = input.DiscountValue > 0 ? now.AddDays(30) : null };
                option.ProductVariants.Add(variant); created.Add((input, variant));
            }
            product.SaleOptions.Add(option);
        }
        return created;
    }

    private async Task AddVariantImages(Product product, List<(AdminVariantInputViewModel Input, ProductVariant Variant)> variants)
    {
        foreach (var item in variants.Where(x => x.Input.Image is not null && x.Input.Image.Length > 0))
            _db.ProductImages.Add(new ProductImage { ProductId = product.Id, VariantId = item.Variant.Id, ImageUrl = await SaveImageAsync(item.Input.Image!), AltText = $"{product.Name} - {item.Input.Color ?? item.Variant.ProductSaleOption.Title}", IsPrimary = false, SortOrder = 1 });
    }

    private async Task<IActionResult> InvalidProductForm(AdminProductCreateViewModel model, string message)
    {
        ModelState.AddModelError(string.Empty, message);
        if (IsAjaxRequest()) return BadRequest(new { success = false, message });
        await Task.CompletedTask;
        return View("Products/Create", model);
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
