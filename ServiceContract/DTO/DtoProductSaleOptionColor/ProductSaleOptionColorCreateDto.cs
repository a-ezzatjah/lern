namespace ServiceContract.DTO.DtoProductSaleOptionColor
{
    public class ProductSaleOptionColorCreateDto
    {
        public int ProductSaleOptionId { get; set; }
        public string Color { get; set; } = null!;
        public string? HexCode { get; set; }
    }
}
