using ServiceContract.DTO.DtoCategory;
using ServiceContract.DTO.DtoProductImage;
using ServiceContract.DTO.DtoProductSaleOption;
using ServiceContract.DTO.DtoProductVariant;
using ServiceContract.DTO.DtoSeo;

namespace ServiceContract.DTO.DtoProduct;

/// <summary>Read model used by the storefront product details page.</summary>
public class ProductPageViewModel
{
    public int Id { get; set; }
    public string Name { get; set; } = null!;
    public string Slug { get; set; } = null!;
    public string? Description { get; set; }
    public string? ShortDescription { get; set; }
    public bool HasDiscount { get; set; }
    public decimal? DiscountPercent { get; set; }
    public DateTime? DiscountEndAt { get; set; }
    public bool IsAvailable { get; set; }
    public int? DefaultVariantId { get; set; }
    public decimal? MinPrice { get; set; }
    public decimal? MaxPrice { get; set; }
    public List<CategoryBriefDto> Categories { get; set; } = new();
    public List<ProductSaleOptionDetailDto> SaleOptions { get; set; } = new();
    public List<ProductVariantDetailDto> ProductVariants { get; set; } = new();
    public List<ProductImageDetailDto> Images { get; set; } = new();
    public List<ProductImageDetailDto> productImage { get => Images; set => Images = value; }
    public SeoDataDto? SeoData { get; set; }
}
