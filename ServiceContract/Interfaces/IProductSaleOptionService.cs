using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ServiceContract.DTO.DtoCommit;
using ServiceContract.DTO.DtoProductSaleOption;

namespace ServiceContract.Interfaces
{
    public interface IProductSaleOptionService
    {

        Task<ServiceResponseDto<ProductSaleOptionListItemDto>> AddProductSaleOptionAsync(ProductSaleOptionCreateDto model);

        Task<ServiceResponseDto<ProductSaleOptionListItemDto>> GetProductSaleOptionByIdAsync(int id);

        Task<ServiceResponseDto<bool>> DeleteProductSaleOptionAsync(int id);

        Task<ServiceResponseDto<ProductSaleOptionListItemDto>> UpdateProductSaleOptionAsync(ProductSaleOptionUpdateDto model);

        

    }
}
