using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Entities;
using ServiceContract.Common;
using ServiceContract.DTO.DtoCommit;
using ServiceContract.DTO.DtoProduct;
using ServiceContract.Quaries;

namespace ServiceContract.Interfaces
{
    public interface IProductService
    {
        public Task<DtoResponse<DtoProductAdminList>> AddProductAsync(DtoproductAdd model);

        public Task<DtoProductDetail?> GetByIdAsync(int productId);

        public Task<Product?> GetEntityByIdAsync(int product);

        public Task<PageResult<DtoProductAdminList>> GetFilterAsync(ProductQuery query);

        public Task<DtoProductAdminList?> GetAdminByIdAsync(int productId);


        public Task<DtoResponse<DtoProductAdminList>> UpdateAsync(DtoProductUpdate model);

        public Task<DtoResponse<bool>> DeleteAsync(int productid);
    }
}
