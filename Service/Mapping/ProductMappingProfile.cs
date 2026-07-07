using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AutoMapper;
using Entities;
using ServiceContract.DTO.DtoProduct;

namespace Service.Mapping
{
    public class ProductMappingProfile : Profile
    {
        public ProductMappingProfile()
        {
            CreateMap<DtoproductAdd, Product>()
                .ForMember(x => x.Name, otp => otp.MapFrom(s => s.Name.Trim()))
                .ForMember(x => x.Slug, otp => otp.MapFrom(s => s.Slug.Trim()))
                .ForMember(x => x.ProductCategories, otp => otp.Ignore())
                .ForMember(x => x.SaleOptions, otp => otp.Ignore())
                .ForMember(x => x.Seo, otp => otp.Ignore())
                .ForMember(x => x.CreatedAt, otp => otp.Ignore())
                .ForMember(x => x.UpdatedAt, otp => otp.Ignore());

            CreateMap<Product, DtoProductAdminList>()
                .ForMember(x => x.CategoriesCount, otp => otp.MapFrom(s => s.ProductCategories.Count))
                .ForMember(x => x.SaleOptionsCount, otp => otp.MapFrom(s => s.SaleOptions.Count));

            CreateMap<Product, DtoProductDetail>()
                .ForMember(x => x.CategoryIds, otp => otp.MapFrom(s => s.ProductCategories.Select(pc => pc.CategoryId)))
                .ForMember(x => x.CategoryNames, otp => otp.MapFrom(s => s.ProductCategories.Select(pc => pc.Category.Name)));

            CreateMap<DtoProductUpdate, Product>()
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
