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
        public Task<ServiceResponseDto<ProductListItemDto>> AddProductAsync(ProductCreateDto model);

        public Task<ProductDetailDto?> GetByIdAsync(int productId);

        public Task<Product?> GetEntityByIdAsync(int product);

        public Task<PageResult<ProductListItemDto>> GetFilterAsync(ProductQuery query);

        public Task<ProductListItemDto?> GetListItemByIdAsync(int productId);

        public Task<ProductUpdateDto?> GetForUpdateAsync(int productId);

        public Task<ServiceResponseDto<ProductListItemDto>> UpdateAsync(ProductUpdateDto model);

        public Task<ServiceResponseDto<bool>> DeleteAsync(int productid);
    }
}
