using Entities.Enums;
using ServiceContract.DTO.DtoProductSaleOptionColor;

namespace ServiceContract.DTO.DtoProductSaleOption
{
    public class ProductSaleOptionDetailDto
    {
        public int Id { get; set; }
        public string Title { get; set; } = null!;
        public EnumSaleType SaleType { get; set; }
        public decimal BasePrice { get; set; }
        public string? ImageUrl { get; set; }
        public string? UnitName { get; set; }
        public string? InputLabel { get; set; }
        public List<ProductSaleOptionColorDetailDto> ProductSaleOptionColors { get; set; } = new();
    }
}
