namespace ServiceContract.DTO.DtoSaleOptionColor
{
    public class SaleOptionColorDetailDto
    {
        public int Id { get; set; }
        public int SaleOptionId { get; set; }
        public string Color { get; set; } = null!;
        public decimal? Price { get; set; }
        public string? HexCode { get; set; }
        public string? ImageUrl { get; set; }
    }
}
