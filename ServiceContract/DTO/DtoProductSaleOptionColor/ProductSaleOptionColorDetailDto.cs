namespace ServiceContract.DTO.DtoProductSaleOptionColor
{
    public class ProductSaleOptionColorDetailDto
    {
        public int Id { get; set; }
        public int ProductSaleOptionId { get; set; }
        public string Color { get; set; } = null!;
        public decimal? Price { get; set; }
        public string? HexCode { get; set; }
        public string? ImageUrl { get; set; }
    }
}
