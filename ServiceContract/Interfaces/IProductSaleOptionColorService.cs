using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ServiceContract.DTO.DtoCommit;
using ServiceContract.DTO.DtoProductSaleOptionColor;
using ServiceContract.Quaries;

namespace ServiceContract.Interfaces
{
    public interface IProductSaleOptionColorService
    {
        Task<ProductSaleOptionColorUpdateDto?> GetProductSaleOptionColorForUpdateAsync(
            int productSaleOptionColorId);

        ServiceResponseDto<ProductSaleOptionColorDetailDto> UpdateProductSaleOptionColor(
            ProductSaleOptionColorUpdateDto model);

        ServiceResponseDto<ProductSaleOptionColorDetailDto> CreateProductSaleOptionColor(
            ProductSaleOptionColorCreateDto model);

        ServiceResponseDto<ProductSaleOptionColorDetailDto> DeleteProductSaleOptionColor(int productSaleOptionColorId);


    }
}
