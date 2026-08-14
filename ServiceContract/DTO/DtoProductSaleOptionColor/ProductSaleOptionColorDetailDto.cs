using ServiceContract.DTO.DtoProductVariant;

namespace ServiceContract.DTO.DtoProductSaleOptionColor
{
    public class ProductSaleOptionColorDetailDto
    {
        public int Id { get; set; }
        public int ProductSaleOptionId { get; set; }
        public string Color { get; set; } = null!;
        public string? HexCode { get; set; }

        public List<ProductVariantDetailDto> ProductVariants { get; set; } = new List<ProductVariantDetailDto>();

    }
}
