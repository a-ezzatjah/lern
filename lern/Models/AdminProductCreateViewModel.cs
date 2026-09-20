using System.ComponentModel.DataAnnotations;

namespace lern.Models;

public class AdminProductCreateViewModel
{
    public int Id { get; set; }
    public IFormFile? PrimaryImage { get; set; }
    public string? ExistingPrimaryImageUrl { get; set; }
    public bool IsEdit => Id > 0;
    [Required(ErrorMessage = "نام محصول را وارد کنید.")]
    public string Name { get; set; } = string.Empty;

    [Required(ErrorMessage = "Slug را وارد کنید.")]
    public string Slug { get; set; } = string.Empty;

    public string? ShortDescription { get; set; }
    public string? Description { get; set; }

    // SEO fields are kept on the product's owned SeoData entity.
    public string? MetaTitle { get; set; }
    public string? MetaDescription { get; set; }
    public string? MetaKeywords { get; set; }
    public string? CanonicalUrl { get; set; }
    public bool IndexPage { get; set; } = true;
    public bool FollowPage { get; set; } = true;

    public List<int> CategoryIds { get; set; } = new();
    public List<AdminProductCategoryOptionViewModel> CategoryOptions { get; set; } = new();
    public bool IsActive { get; set; } = true;

    public decimal? ProductDiscountValue { get; set; }
    public int? ProductDiscountType { get; set; }

    public List<AdminVariantInputViewModel> Variants { get; set; } = new();
}

public class AdminProductCategoryOptionViewModel
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public int? ParentId { get; set; }
    public int SortOrder { get; set; }
}

public class AdminVariantInputViewModel
{
    public string SaleTitle { get; set; } = string.Empty;
    public int SaleType { get; set; } = 4;
    public string? Color { get; set; }
    public string? HexCode { get; set; }
    public decimal Price { get; set; }
    public int StockQuantity { get; set; } = 10;
    public decimal? DiscountValue { get; set; }
    public int? DiscountType { get; set; }
    public IFormFile? Image { get; set; }
}
