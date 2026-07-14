using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DTO;
using ServiceContract.DTO.DtoProductSaleOption;

namespace ServiceContract.DTO.DtoProduct
{
    public class ProductDetailDto
    {


        public string? Name { get; set; }
        public string? Slug { get; set; }
        public string? Description { get; set; }

        public bool IsActive { get; set; }

        public decimal? DiscountValue { get; set; }
        public DisconTypeEnum? DiscountType { get; set; }

        public bool HasDiscount { get; set; }

        public List<int> CategoryIds { get; set; } = new();
        public List<string> CategoryNames { get; set; } = new();
        public List<ProductSaleOptionDetailDto> SaleOptions { get; set; } = new();

        public DateTime CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }






    }
}
