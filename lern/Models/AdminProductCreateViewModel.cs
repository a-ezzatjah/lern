using System.ComponentModel.DataAnnotations;

namespace lern.Models;

public class AdminProductCreateViewModel
{
    [Required(ErrorMessage = "نام محصول را وارد کنید.")]
    public string Name { get; set; } = string.Empty;

    [Required(ErrorMessage = "Slug را وارد کنید.")]
    public string Slug { get; set; } = string.Empty;

    public string? ShortDescription { get; set; }
    public bool IsActive { get; set; } = true;

    public decimal? ProductDiscountValue { get; set; }
    public int? ProductDiscountType { get; set; }

    public List<AdminVariantInputViewModel> Variants { get; set; } = new();
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
