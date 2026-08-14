using AutoMapper;
using Entities;
using ServiceContract.DTO.DtoProductVariant;

namespace Service.Mapping
{
    public class ProductVariantMappingProfile : Profile
    {
        public ProductVariantMappingProfile()
        {
            CreateMap<ProductVariantCreateDto, ProductVariant>()
                .ForMember(x => x.Id, otp => otp.Ignore())
                .ForMember(x => x.ProductSaleOption, otp => otp.Ignore())
                .ForMember(x => x.saleoptioncolor, otp => otp.Ignore())
                .ForMember(x => x.ProductImages, otp => otp.Ignore());

            CreateMap<ProductVariantUpdateDto, ProductVariant>()
                .ForMember(x => x.Id, otp => otp.Ignore())
                .ForMember(x => x.ProductSaleOption, otp => otp.Ignore())
                .ForMember(x => x.saleoptioncolor, otp => otp.Ignore())
                .ForMember(x => x.ProductImages, otp => otp.Ignore());

            CreateMap<ProductVariant, ProductVariantDetailDto>()
                .ForMember(
                    x => x.ProductId,
                    otp => otp.MapFrom(s => s.ProductSaleOption.ProductId));

            CreateMap<ProductVariant, ProductVariantListItemDto>();

            CreateMap<ProductVariant, ProductVariantUpdateDto>()
                .ForMember(
                    x => x.ProductId,
                    otp => otp.MapFrom(s => s.ProductSaleOption.ProductId))
                .ForMember(
                    x => x.MinQuantity,
                    otp => otp.MapFrom(s => s.ProductSaleOption.MinQuantity))
                .ForMember(
                    x => x.MaxQuantity,
                    otp => otp.MapFrom(s => s.ProductSaleOption.MaxQuantity))
                .ForMember(
                    x => x.Step,
                    otp => otp.MapFrom(s => s.ProductSaleOption.Step));
        }
    }
}
