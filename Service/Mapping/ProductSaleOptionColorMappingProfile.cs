using AutoMapper;
using Entities;
using ServiceContract.DTO.DtoProductSaleOptionColor;

namespace Service.Mapping
{
    public class ProductSaleOptionColorMappingProfile : Profile
    {
        public ProductSaleOptionColorMappingProfile()
        {
            CreateMap<ProductSaleOptionColorCreateDto, ProductSaleOptionColor>()
                .ForMember(x => x.Id, otp => otp.Ignore())
                .ForMember(x => x.ProductSaleOption, otp => otp.Ignore())
                .ForMember(x => x.ProductVariants, otp => otp.Ignore());

            CreateMap<ProductSaleOptionColorUpdateDto, ProductSaleOptionColor>()
                .ForMember(x => x.Id, otp => otp.Ignore())
                .ForMember(x => x.ProductSaleOptionId, otp => otp.Ignore())
                .ForMember(x => x.ProductSaleOption, otp => otp.Ignore())
                .ForMember(x => x.ProductVariants, otp => otp.Ignore());

            CreateMap<ProductSaleOptionColor, ProductSaleOptionColorDetailDto>();
            CreateMap<ProductSaleOptionColor, ProductSaleOptionColorListItemDto>()
                .ForMember(x => x.FinalPrice, otp => otp.Ignore());
            CreateMap<ProductSaleOptionColor, ProductSaleOptionColorUpdateDto>();
        }
    }
}
