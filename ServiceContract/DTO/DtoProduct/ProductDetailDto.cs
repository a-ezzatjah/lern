using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DTO;
using ServiceContract.DTO.DtoProductImage;
using ServiceContract.DTO.DtoProductSaleOption;
using ServiceContract.DTO.DtoProductSaleOptionColor;
using ServiceContract.DTO.DtoProductVariant;
using ServiceContract.DTO.DtoSeo;

namespace ServiceContract.DTO.DtoProduct
{
    public class ProductDetailDto
    {

        public int Id { get; set; }

        public string Name { get; set; }
        public string Slug { get; set; }
        public string? Description { get; set; }

        public string? ShortDescription { get; set; }

        public bool HasDiscount { get; set; } 

        public decimal? DiscountPercent { get; set; }

        public DateTime? DiscountEndAt { get; set; }
        
        public List<CategoryBriefDto> Categories { get; set; } = new();

        public List<ProductSaleOptionDetailDto> SaleOptions { get; set; } = new();

        public List<ProductSaleOptionColorDetailDto> ProductSaleOptionColors { get; set; } = new();

        public List<ProductVariantDetailDto> ProductVariants { get; set; } = new();

        public List<ProductImageDetailDto> productImage { get; set; } = new();

        public SeoDataDto? SeoData { get; set; }


    // public ReviewSummaryDto ReviewSummary { get; set; } = new();

    // public List<ProductReviewDto> Reviews { get; set; } = new();

    // public List<ProductCardDto> RelatedProducts { get; set; } = new();


    }
}
