using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DTO;
using Entities;
using ServiceContract.Common;
using ServiceContract.DTO;
using ServiceContract.Quaries;

namespace ServiceContract.Interfaces
{
    public interface IProductService
    {
        public DtoResponse<DtoProduct> AddProduct(DtoproductAdd model);

        public DtoProduct GetById(int productId);


        public Product? GetEntityById(int product);


        public Task<PageResult<DtoProduct>> GetFilterAsync(ProductQuery query);

        public List<DtoSearchOption> GetSelect();

        public DtoResponse<DtoProduct> Update(DtoProductUpdate model);

        public DtoResponse<bool> Delete(int productid);

        public string GetBranchName(int? branchid);


    }
}
