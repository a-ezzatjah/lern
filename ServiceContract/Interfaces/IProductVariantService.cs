using ServiceContract.DTO.DtoCommit;
using ServiceContract.DTO.DtoProductVariant;

namespace ServiceContract.Interfaces
{
    public interface IProductVariantService
    {
        Task<ServiceResponseDto<ProductVariantDetailDto>> AddProductVariantAsync(ProductVariantCreateDto model);
        Task<ServiceResponseDto<ProductVariantDetailDto>> GetProductVariantByIdAsync(int id);
        Task<ServiceResponseDto<IReadOnlyList<ProductVariantListItemDto>>> GetProductVariantsByProductIdAsync(int productId);
        Task<ServiceResponseDto<ProductVariantDetailDto>> UpdateProductVariantAsync(ProductVariantUpdateDto model);
        Task<ServiceResponseDto<bool>> DeleteProductVariantAsync(int id);
    }
}
