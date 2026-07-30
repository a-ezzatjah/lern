namespace ServiceContract.DTO.DtoProductImage
{
    public class ProductImageDetailDto
    {
        public int Id { get; set; }
      
        public string? ImageUrl { get; set; }
        public string? AltText { get; set; }
        public int SortOrder { get; set; }
        public bool IsPrimary { get; set; }
    }
}
