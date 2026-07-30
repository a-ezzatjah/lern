using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AutoMapper;
using Entities;
using ServiceContract.DTO.DtoProduct;
using ServiceContract.DTO.DtoProductSaleOption;
using ServiceContract.DTO.DtoSeo;

namespace Service.Mapping
{
    public class ProductMappingProfile : Profile
    {
        public ProductMappingProfile()
        {
            CreateMap<ProductCreateDto, Product>()
                .ForMember(x => x.Name, otp => otp.MapFrom(s => s.Name.Trim()))
                .ForMember(x => x.Slug, otp => otp.MapFrom(s => s.Slug.Trim()))
                .ForMember(x => x.ProductCategories, otp => otp.Ignore())
                .ForMember(x => x.Seo, otp => otp.Ignore())
                .ForMember(x => x.CreatedAt, otp => otp.Ignore())
                .ForMember(x => x.UpdatedAt, otp => otp.Ignore());

            CreateMap<ProductSaleOptionCreateDto, ProductSaleOption>();

             CreateMap<SeoDataDto,SeoData>();

            CreateMap<ProductSaleOptionUpdateDto, ProductSaleOption>()
                .ForMember(x => x.Id, otp => otp.Ignore())
                .ForMember(x => x.ProductId, otp => otp.Ignore())
                .ForMember(x => x.Product, otp => otp.Ignore())
                .ForMember(x => x.SaleOptionColors, otp => otp.Ignore())
                .ForMember(x => x.ProductVariants, otp => otp.Ignore());

            CreateMap<ProductSaleOption, ProductSaleOptionDetailDto>();
            CreateMap<ProductSaleOption, ProductSaleOptionUpdateDto>();

            CreateMap<ProductSaleOption, ProductSaleOptionListItemDto>()
                .ForMember(
                    x => x.ProductSaleOptionColors,
                    otp => otp.MapFrom(s => s.SaleOptionColors));

            CreateMap<Product, ProductListItemDto>()
                .ForMember(x => x.CategoriesCount, otp => otp.MapFrom(s => s.ProductCategories.Count))
                .ForMember(x => x.SaleOptionsCount, otp => otp.MapFrom(s => s.SaleOptions.Count))
                .ForMember(x => x.CategoryNames, otp => otp.MapFrom(s => s.ProductCategories.Select(pc => pc.Category.Name)))
                .ForMember(x => x.SaleOptionTitles, otp => otp.MapFrom(s => s.SaleOptions.Select(pc => pc.Title)))
                .ForMember(x => x.SaleOptions, otp => otp.MapFrom(s => s.SaleOptions));

            CreateMap<Product, ProductDetailDto>()
                .ForMember(x => x.CategoryIds, otp => otp.MapFrom(s => s.ProductCategories.Select(pc => pc.CategoryId)))
                .ForMember(x => x.CategoryNames, otp => otp.MapFrom(s => s.ProductCategories.Select(pc => pc.Category.Name)));
                
            
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
