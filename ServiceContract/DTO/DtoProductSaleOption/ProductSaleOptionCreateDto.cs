using Entities.Enums;

namespace ServiceContract.DTO.DtoProductSaleOption
{
    public class ProductSaleOptionCreateDto
    {
        public string Title { get; set; } = null!;
        public int ProductId { get; set; }
        public EnumSaleType SaleType { get; set; }
 
        public string? InputLabel { get; set; }
        public string? UnitName { get; set; }
        public decimal? MinQuantity { get; set; }
        public decimal? MaxQuantity { get; set; }
        public decimal Step { get; set; } = 1;

    }
}
