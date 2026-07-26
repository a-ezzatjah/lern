using DTO;

namespace ServiceContract.DTO.DtoProductVariant
{
    public class ProductVariantUpdateDto
    {
        public int Id { get; set; }
        public int ProductId { get; set; }
        public int ProductSaleOptionId { get; set; }
        public int ProductSaleOptionColorId { get; set; }
        public string Sku { get; set; } = null!;
        public decimal Price { get; set; }
        public decimal? DiscountValue { get; set; }
        public DisconTypeEnum? DisconType { get; set; }
        public DateTime? DiscountStartAt { get; set; }
        public DateTime? DiscountEndAt { get; set; }
        public int StockQuantity { get; set; }
        public int ReservedQuantity { get; set; }
        public decimal? MinQuantity { get; set; }
        public decimal? MaxQuantity { get; set; }
        public decimal Step { get; set; } = 1;
    }
}
