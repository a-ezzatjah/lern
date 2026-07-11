namespace ServiceContract.DTO.DtoSaleOptionColor
{
    public class DtoUpdateSaleOptionColor
    {
        public int Id { get; set; }
        public string Color { get; set; } = null!;
        public decimal? Price { get; set; }
        public string? HexCode { get; set; }
        public string? ImageUrl { get; set; }
    }
}
