using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Entities;
using Entities.Enums;
using ServiceContract.DTO.DtoProductSaleOptionColor;
using ServiceContract.DTO.DtoProductVariant;

namespace ServiceContract.DTO.DtoProductSaleOption
{
    public class ProductSaleOptionListItemDto
    {

        public int Id { get; set; }

        public string Title { get; set; } = null!;

        public EnumSaleType SaleType { get; set; }

        public List<ProductSaleOptionColorListItemDto> ProductSaleOptionColors { get; set; } = new();
        
        public List<ProductVariantListItemDto> ProductVariants { get; set; } = new List<ProductVariantListItemDto>();


    }
}
