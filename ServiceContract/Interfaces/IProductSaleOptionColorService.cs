using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ServiceContract.DTO.DtoCommit;
using ServiceContract.DTO.DtoSaleOptionColor;
using ServiceContract.Quaries;

namespace ServiceContract.Interfaces
{
    public interface IProductSaleOptionColorService
    {

        public ServiceResponseDto<SaleOptionColorDetailDto> UpdateProductColor(SaleOptionColorUpdateDto model);

        public ServiceResponseDto<SaleOptionColorDetailDto>CreateProdactColor(SaleOptionColorCreateDto model);


        public ServiceResponseDto<SaleOptionColorDetailDto> DeleteProdactColor(int  ProductColorid);


    }
}
