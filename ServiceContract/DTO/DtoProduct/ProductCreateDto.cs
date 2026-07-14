using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DTO;
using ServiceContract.DTO.DtoProductSaleOption;

namespace ServiceContract.DTO.DtoProduct
{
    public class ProductCreateDto
    {
        public string Name { get; set; } = null!;
        public string Slug { get; set; } = null!;

        public string? Description { get; set; }
        public decimal? DiscountValue { get; set; }
        public DisconTypeEnum? DiscountType { get; set; }
        public bool IsActive { get; set; } = true;
        public List<int> CategoryIds { get; set; } = new();
        public List<ProductSaleOptionCreateDto> SaleOptions { get; set; } = new();

    }
}
