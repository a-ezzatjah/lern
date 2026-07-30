namespace ServiceContract.DTO.DtoProductImage
{
    public class ProductImageUpdateDto
    {
        public int Id { get; set; }
        public int? ProductId { get; set; }
        public int? VariantId { get; set; }
        public string? ImageUrl { get; set; }
        public string? AltText { get; set; }
        public int SortOrder { get; set; }
        public bool IsPrimary { get; set; }
    }
}
