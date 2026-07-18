using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DTO;
using ServiceContract.DTO.DtoProductSaleOption;

namespace ServiceContract.DTO.DtoProduct
{
    public class ProductPatchFieldDto
    {
        public int? Id { get; set; }

        public PatchField<string?> Name { get; set; } = null!;

        public PatchField<string?> Slug { get; set; } = null!;
        public PatchField<string?> Description { get; set; } = null!;

        public PatchField<bool> IsActive { get; set; } = new();

        public PatchField<decimal?> DiscountValue { get; set; } 
        public DisconTypeEnum? DiscountType { get; set; }

        public  List<int> CategoryIds { get; set; } = new();
        public PatchField<List<ProductSaleOptionPatchFieldDto>>? SaleOptions { get; set; } 
    }
}
