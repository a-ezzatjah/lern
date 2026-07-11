using DTO;

namespace ServiceContract.DTO.DtoProduct
{
    public class DtoProductAdminList
    {
        public int Id { get; set; }

        public string Name { get; set; } = null!;
        public string Slug { get; set; } = null!;

        public bool IsActive { get; set; }


        public decimal? DiscountValue { get; set; }
        public DisconTypeEnum? DiscountType { get; set; }

        public bool HasDiscount { get; set; }

        public int CategoriesCount { get; set; }
        public int SaleOptionsCount { get; set; }

        public List<string> CategoriesName { get; set; } = new();
        public List<string> SaleOptionTitle { get; set; } = new();

        public DateTime CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }
    }
}
