namespace ServiceContract.DTO.DtoCategory
{
    public class CategorySelectItemDto
    {
        public int Id { get; set; }
        public string Name { get; set; } = null!;
        public int? ParentId { get; set; }
    }
}
