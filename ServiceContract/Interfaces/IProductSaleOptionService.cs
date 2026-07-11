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

        public DtoResponse<DtoProductAdminSaleOption> AddProductSaleOptionAsync(DtoAddProductSaleOption model);

        public DtoResponse<DtoProductAdminSaleOption> GetProductSaleOptionByIdAsync(int id);

        public DtoResponse<DtoProductAdminSaleOption> DeleteProductSaleOptionAsync(int id);

        public DtoResponse<DtoProductAdminSaleOption> UpdateProductSaleOptionAsync(UpdateDtoProductSaleOption model);

        

    }
}
