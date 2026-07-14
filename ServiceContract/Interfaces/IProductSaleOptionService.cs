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

        public ServiceResponseDto<ProductSaleOptionAdminDto> AddProductSaleOptionAsync(ProductSaleOptionCreateDto model);

        public ServiceResponseDto<ProductSaleOptionAdminDto> GetProductSaleOptionByIdAsync(int id);

        public ServiceResponseDto<ProductSaleOptionAdminDto> DeleteProductSaleOptionAsync(int id);

        public ServiceResponseDto<ProductSaleOptionAdminDto> UpdateProductSaleOptionAsync(ProductSaleOptionPatchFieldDto model);

        

    }
}
