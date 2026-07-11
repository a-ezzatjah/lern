namespace ServiceContract.DTO.DtoSaleOptionColor
{
    public class DtoProductAdminSaleOptionColor
    {

        public int Id { get; set; }

        public string Color { get; set; } = null!;

        public string? HexCode { get; set; }

        public decimal? Price { get; set; }

        public decimal FinalPrice { get; set; }

        public string? ImageUrl { get; set; }




    }
}