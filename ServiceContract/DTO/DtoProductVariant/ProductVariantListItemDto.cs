using ServiceContract.DTO.DtoProductImage;

namespace ServiceContract.DTO.DtoProductVariant
{
    public class ProductVariantListItemDto
    {
        public int Id { get; set; }
        public string Sku { get; set; } = null!;
        public decimal Price { get; set; }
        public decimal? DiscountValue { get; set; }
        public int StockQuantity { get; set; }
        public int ReservedQuantity { get; set; }
        public int AvailableQuantity { get; set; }
        public List<ProductImageListItemDto> ProductImages { get; set; } = new List<ProductImageListItemDto>();
    }
}
