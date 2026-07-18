namespace ServiceContract.DTO.DtoCategory
{
    public class CategoryUpdateDto
    {
        public int Id { get; set; }
        public string Name { get; set; } = null!;
        public string Slug { get; set; } = null!;
        public int? ParentId { get; set; }
        public int? SortOrder { get; set; }
    }
}
