using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DTO;
using ServiceContract.DTO.DtoProductSaleOption;

namespace ServiceContract.DTO.DtoProduct
{
    public class DtoProductUpdate
    {
        public int Id { get; set; }

        public string? Name { get; set; }
        public string? Slug { get; set; }
        public string? Description { get; set; }

        public bool IsActive { get; set; } = true;

        public decimal? DiscountValue { get; set; }
        public DisconTypeEnum? DiscountType { get; set; }

        public List<int> CategoryIds { get; set; } = new();
        public List<UpdateDtoProductSaleOption> SaleOptions { get; set; } = new();
    }
}
