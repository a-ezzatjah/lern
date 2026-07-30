using Entities;
using ServiceContract.DTO.DtoProductVariant;

namespace ServiceContract.DTO.DtoProductSaleOptionColor
{
    public class ProductSaleOptionColorListItemDto
    {
        public int Id { get; set; }
        public string Color { get; set; } = null!;
        public string? HexCode { get; set; }
        public List<ProductVariantListItemDto> ProductVariants { get; set; } = new List<ProductVariantListItemDto>();

    }
}
