using Entities.Enums;
using ServiceContract.DTO.DtoSaleOptionColor;

namespace ServiceContract.DTO.DtoProductSaleOption
{
    public class DtoProductSaleOptionView
    {
        public int Id { get; set; }
        public string Title { get; set; } = null!;
        public EnumSaleType SaleType { get; set; }
        public decimal BasePrice { get; set; }
        public string? ImageUrl { get; set; }
        public string? UnitName { get; set; }
        public string? InputLabel { get; set; }
        public decimal? MinQuantity { get; set; }
        public decimal? MaxQuantity { get; set; }
        public decimal Step { get; set; }
        public decimal? FixedWeight { get; set; }
        public decimal? FixedLength { get; set; }
        public decimal? FixedWidth { get; set; }
        public decimal? FixedHeight { get; set; }
        public decimal? PerUnitWeight { get; set; }
        public decimal? PerUnitLength { get; set; }
        public decimal? PerUnitWidth { get; set; }
        public decimal? PerUnitHeight { get; set; }
        public List<DtoSaleOptionColorView> SaleOptionColors { get; set; } = new();
    }
}
