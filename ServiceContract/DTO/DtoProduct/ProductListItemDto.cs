using DTO;
using Microsoft.Identity.Client;
using ServiceContract.DTO.DtoProductSaleOption;

namespace ServiceContract.DTO.DtoProduct
{
    public class ProductListItemDto
    {
        public int Id { get; set; }

        public string Name { get; set; } = null!;
        public string Slug { get; set; } = null!;

        public bool IsActive { get; set; }

        public decimal? DiscountValue { get; set; }
        public DisconTypeEnum? DiscountType { get; set; }

        public bool HasDiscount { get; set; }

        public decimal? DiscountPercent { get; set; }

         public DateTime? DiscountStartAt { get; set; }
        public DateTime? DiscountEndAt { get; set; }

        public List<string> CategoryNames { get; set; } = new();
        public List<string> SaleOptionTitles { get; set; } = new();

        public List<string> ColorName {get;set;} = new();

        public decimal? MinPrice { get; set; } 
        
        public int StockQuantity { get; set; }


        public DateTime CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }


    }
}
