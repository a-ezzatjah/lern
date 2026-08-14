using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Intrinsics.X86;
using System.Text;
using System.Threading.Tasks;
using AutoMapper;
using Entities;
using ServiceContract.DTO.DtoProduct;
using ServiceContract.DTO.DtoProductImage;
using ServiceContract.DTO.DtoProductSaleOption;
using ServiceContract.DTO.DtoProductSaleOptionColor;
using ServiceContract.DTO.DtoProductVariant;
using ServiceContract.DTO.DtoSeo;

namespace Service.Mapping
{
    public class ProductMappingProfile : Profile
    {
        public ProductMappingProfile()
        {
        
        //ProductCreateDtoMapping
        CreateMap<ProductCreateDto, Product>()
            .ForMember(x => x.Name, otp => otp.MapFrom(s => s.Name.Trim()))
            .ForMember(x => x.Slug, otp => otp.MapFrom(s => s.Slug.Trim().ToLowerInvariant()))
            .ForMember(x => x.ProductCategories, otp => otp.Ignore());
        CreateMap<SeoDataDto, SeoData>();
               


        //ProductDetailDtoMapping
        CreateMap<ProductSaleOptionColor, ProductSaleOptionColorDetailDto>();
        CreateMap<ProductVariant, ProductVariantDetailDto>();
        CreateMap<ProductImage, ProductImageDetailDto>();      
        CreateMap<Category , CategoryBriefDto>();
        CreateMap<Product, ProductDetailDto>()
            .ForMember(x=>x.HasDiscount , otp => otp
            .MapFrom(s=>s.DiscountValue.HasValue && s.DiscountValue >=0 && s.DiscountType.HasValue))
            .ForMember(x=>x.Categories,otp => otp.MapFrom(s=>s.ProductCategories.Select(pc=>pc.Category)));
            

        //ProductListItemDto
        CreateMap<Product , ProductListItemDto>()
            .ForMember(x=>x.HasDiscount,otp=>otp.MapFrom(s=>s.DiscountValue.HasValue && s.DiscountValue.Value > 0))
            .ForMember(x=>x.CategoryNames,otp=>otp.MapFrom(s=>s.ProductCategories.Select(s=>s.Category.Name)))
            .ForMember(x=>x.SaleOptionTitles,otp=>otp.MapFrom(s=>s.SaleOptions.Select(s=>s.Title)))
            .ForMember(x=>x.ColorName,otp=>otp
            .MapFrom(s=>s.SaleOptions.Select(y=>y.SaleOptionColors.Where(x=>x.Color != null).Select(s=>s.Color))));






                
            
            CreateMap<Product, ProductUpdateDto>()
                .ForMember(x => x.CategoryIds, otp => otp.MapFrom(s => s.ProductCategories.Select(pc => pc.CategoryId)));

            CreateMap<ProductUpdateDto, Product>()
                .ForMember(x => x.Id, y => y.Ignore())
                .ForMember(x => x.Name, otp => otp.MapFrom(s => s.Name.Trim()))
                .ForMember(x => x.Slug, otp => otp.MapFrom(s => s.Slug.Trim()))
                .ForMember(x => x.ProductCategories, otp => otp.Ignore())
                .ForMember(x => x.SaleOptions, otp => otp.Ignore())
                .ForMember(x => x.Seo, otp => otp.Ignore())
                .ForMember(x => x.CreatedAt, otp => otp.Ignore())
                .ForMember(x => x.UpdatedAt, otp => otp.Ignore());


        }
    }
}
