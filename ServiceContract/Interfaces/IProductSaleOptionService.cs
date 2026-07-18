using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ServiceContract.DTO.DtoCommit;
using ServiceContract.DTO.DtoProductSaleOption;
using ServiceContract.DTO.DtoSaleOptionColor;

namespace ServiceContract.Interfaces
{
    public interface IProductSaleOptionService
    {

        Task<ServiceResponseDto<ProductSaleOptionAdminDto>> AddProductSaleOptionAsync(ProductSaleOptionCreateDto model);

        Task<ServiceResponseDto<ProductSaleOptionAdminDto>> GetProductSaleOptionByIdAsync(int id);

        Task<ServiceResponseDto<bool>> DeleteProductSaleOptionAsync(int id);

        Task<ServiceResponseDto<ProductSaleOptionAdminDto>> UpdateProductSaleOptionAsync(ProductSaleOptionPatchFieldDto model);

        

    }
}
