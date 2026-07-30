using AutoMapper;
using Entities;
using ServiceContract.DTO.DtoProductImage;

namespace Service.Mapping
{
    public class ProductImageMappingProfile : Profile
    {
        public ProductImageMappingProfile()
        {
            CreateMap<ProductImageCreateDto, ProductImage>()
                .ForMember(x => x.Id, otp => otp.Ignore())
                .ForMember(x => x.Product, otp => otp.Ignore())
                .ForMember(x => x.Variant, otp => otp.Ignore());

            CreateMap<ProductImageUpdateDto, ProductImage>()
                .ForMember(x => x.Id, otp => otp.Ignore())
                .ForMember(x => x.Product, otp => otp.Ignore())
                .ForMember(x => x.Variant, otp => otp.Ignore());

            CreateMap<ProductImage, ProductImageDetailDto>();
            CreateMap<ProductImage, ProductImageListItemDto>();
        }
    }
}
