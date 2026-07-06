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

        public Task<DtoProductAdminList?> GetByIdAsync(int productId);

        public Task<Product?> GetEntityByIdAsync(int product);

        public Task<PageResult<DtoProductAdminList>> GetFilterAsync(ProductQuery query);

        public Task<List<DtoSearchOption>> GetSelectAsync();

        public Task<DtoResponse<DtoProductAdminList>> UpdateAsync(DtoProductUpdate model);

        public Task<DtoResponse<bool>> DeleteAsync(int productid);
    }
}
