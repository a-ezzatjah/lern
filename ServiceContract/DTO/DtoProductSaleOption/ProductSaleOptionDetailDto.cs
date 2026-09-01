using Entities.Enums;
using ServiceContract.DTO.DtoProductVariant;
using ServiceContract.DTO.DtoProductSaleOptionColor;

namespace ServiceContract.DTO.DtoProductSaleOption
{
    public class ProductSaleOptionDetailDto
    {
        public int Id { get; set; }
        public string Title { get; set; } = null!;
        public EnumSaleType SaleType { get; set; }
        public string? UnitName { get; set; }
        public string? InputLabel { get; set; }
        public int? MinQuantity { get; set; }
        public int? MaxQuantity { get; set; }
        public int Step { get; set; } = 1;
        public List<ProductVariantDetailDto> ProductVariants { get; set; } = new();
        public List<ProductSaleOptionColorDetailDto> ProductSaleOptionColors { get; set; } = new();
    }
}
