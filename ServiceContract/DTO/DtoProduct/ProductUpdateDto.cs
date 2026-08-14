using DTO;
using Entities;
using ServiceContract.DTO.DtoProductSaleOption;

namespace ServiceContract.DTO.DtoProduct
{
    public class ProductUpdateDto
    {
        public int Id { get; set; }
        public string Name { get; set; } = null!;
        public string Slug { get; set; } = null!;

        public string? Description { get; set; }

        public bool IsActive { get; set; }

        public decimal? DiscountValue { get; set; }

        public DisconTypeEnum? DiscountType { get; set; }

        public DateTime? DiscountStartAt { get; set; }
        public DateTime? DiscountEndAt { get; set; }

        public SeoData? seoData{get;set;}

        public List<int> CategoryIds { get; set; } = new();

    }
}
