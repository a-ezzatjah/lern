using ServiceContract.DTO.DtoCommit;
using ServiceContract.DTO.DtoSaleOptionColor;

namespace ServiceContract.Interfaces
{
    public interface IProductSaleOptionColorService
    {
        ServiceResponseDto<SaleOptionColorDetailDto> UpdateProductColor(SaleOptionColorUpdateDto model);
        ServiceResponseDto<SaleOptionColorDetailDto> CreateProdactColor(SaleOptionColorCreateDto model);
        ServiceResponseDto<SaleOptionColorDetailDto> DeleteProdactColor(int productColorId);
    }
}
