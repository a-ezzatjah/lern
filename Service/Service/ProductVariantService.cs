using AutoMapper;
using Entities;
using ServiceContract.DTO.DtoCommit;
using ServiceContract.DTO.DtoProductVariant;
using ServiceContract.Interfaces;

namespace Service.Service
{
    public class ProductVariantService : IProductVariantService
    {
        private readonly ShopDbContext _shopDbContext;
        private readonly IMapper _mapper;

        public ProductVariantService(ShopDbContext shopDbContext, IMapper mapper)
        {
            _shopDbContext = shopDbContext;
            _mapper = mapper;
        }

        public Task<ServiceResponseDto<ProductVariantDetailDto>> AddProductVariantAsync(ProductVariantCreateDto model)
        {
            throw new NotImplementedException();
        }

        public Task<ServiceResponseDto<ProductVariantDetailDto>> GetProductVariantByIdAsync(int id)
        {
            throw new NotImplementedException();
        }

        public Task<ServiceResponseDto<IReadOnlyList<ProductVariantListItemDto>>> GetProductVariantsByProductIdAsync(
            int productId)
        {
            throw new NotImplementedException();
        }

        public Task<ServiceResponseDto<ProductVariantDetailDto>> UpdateProductVariantAsync(ProductVariantUpdateDto model)
        {
            throw new NotImplementedException();
        }

        public Task<ServiceResponseDto<bool>> DeleteProductVariantAsync(int id)
        {
            throw new NotImplementedException();
        }
    }
}
