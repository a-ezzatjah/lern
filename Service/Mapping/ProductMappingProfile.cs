using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AutoMapper;
using Entities;
using ServiceContract.DTO.DtoProduct;
using ServiceContract.DTO.DtoProductSaleOption;
using ServiceContract.DTO.DtoSaleOptionColor;
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

            CreateMap<ProductSaleOptionPatchFieldDto, ProductSaleOption>()
                .ForMember(x => x.Id, otp => otp.Ignore())
                .ForMember(x => x.ProductId, otp => otp.Ignore())
                .ForMember(x => x.Product, otp => otp.Ignore())
                .ForMember(x => x.SaleOptionColors, otp => otp.MapFrom(s => s.SaleOptionColors));

            CreateMap<SaleOptionColorCreateDto, SaleOptionColor>()
                .ForMember(x => x.Id, otp => otp.Ignore())
                .ForMember(x => x.SaleOptionId, otp => otp.Ignore())
                .ForMember(x => x.ProductSaleOption, otp => otp.Ignore());

            CreateMap<SaleOptionColorPatchFieldDto, SaleOptionColor>()
                .ForMember(x => x.Id, otp => otp.Ignore())
                .ForMember(x => x.SaleOptionId, otp => otp.Ignore())
                .ForMember(x => x.ProductSaleOption, otp => otp.Ignore());

            CreateMap<ProductSaleOption, ProductSaleOptionDetailDto>();

            CreateMap<SaleOptionColor, SaleOptionColorDetailDto>();

            CreateMap<ProductSaleOption, ProductSaleOptionAdminDto>()
                .ForMember(x => x.Colors, otp => otp.MapFrom(s => s.SaleOptionColors));

            CreateMap<SaleOptionColor, SaleOptionColorAdminDto>();

            CreateMap<Product, ProductAdminListItemDto>()
                .ForMember(x => x.CategoriesCount, otp => otp.MapFrom(s => s.ProductCategories.Count))
                .ForMember(x => x.SaleOptionsCount, otp => otp.MapFrom(s => s.SaleOptions.Count))
                .ForMember(x => x.CategoriesName, otp => otp.MapFrom(s => s.ProductCategories.Select(pc => pc.Category.Name)))
                .ForMember(x => x.SaleOptionTitle, otp => otp.MapFrom(s => s.SaleOptions.Select(pc => pc.Title)))
                .ForMember(x => x.SaleOptions, otp => otp.MapFrom(s => s.SaleOptions));

            CreateMap<Product, ProductDetailDto>()
                .ForMember(x => x.CategoryIds, otp => otp.MapFrom(s => s.ProductCategories.Select(pc => pc.CategoryId)))
                .ForMember(x => x.CategoryNames, otp => otp.MapFrom(s => s.ProductCategories.Select(pc => pc.Category.Name)));
                
            
            CreateMap<ProductPatchFieldDto, Product>()
                .ForMember(x => x.Id, y => y.Ignore())
                .ForMember(x => x.Name, otp => { otp.PreCondition(s => !string.IsNullOrWhiteSpace(s.Name)); otp.MapFrom(s => s.Name!.Trim()); })
                .ForMember(x => x.Slug, otp => { otp.PreCondition(s => !string.IsNullOrWhiteSpace(s.Slug)); otp.MapFrom(s => s.Slug!.Trim()); })
                .ForMember(x => x.Description, otp => { otp.PreCondition(s => s.Description != null); otp.MapFrom(s => s.Description); })
                .ForMember(x => x.ProductCategories, otp => otp.Ignore())
                .ForMember(x => x.SaleOptions, otp => otp.Ignore())
                .ForMember(x => x.Seo, otp => otp.Ignore())
                .ForMember(x => x.CreatedAt, otp => otp.Ignore())
                .ForMember(x => x.UpdatedAt, otp => otp.Ignore());
        }
    }
}
