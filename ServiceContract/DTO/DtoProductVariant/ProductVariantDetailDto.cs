using DTO;
using ServiceContract.DTO.DtoProductImage;

namespace ServiceContract.DTO.DtoProductVariant
{
    public class ProductVariantDetailDto
    {
        public int Id { get; set; }
        public int ProductId { get; set; }
        public int ProductSaleOptionId { get; set; }
        public int? ProductSaleOptionColorId { get; set; }
        public string Sku { get; set; } = null!;
        public decimal Price { get; set; }
        public decimal? DiscountValue { get; set; }
        public DisconTypeEnum? DisconType { get; set; }
        public DateTime? DiscountStartAt { get; set; }
        public DateTime? DiscountEndAt { get; set; }
        public int StockQuantity { get; set; }
        public int ReservedQuantity { get; set; }
        public List<ProductImageDetailDto> ProductImages { get; set; } = new List<ProductImageDetailDto>();
       
    }
}
