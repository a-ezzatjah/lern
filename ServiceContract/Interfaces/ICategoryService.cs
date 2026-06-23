using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ServiceContract.Common;
using ServiceContract.DTO.DtoCategory;
using ServiceContract.DTO.DtoCommit;
using ServiceContract.Queries;

namespace ServiceContract.Interfaces
{
    public interface ICategoryService
    {

        public Task<DtoResponse<DtoCategory>> AddCategoryAsync(AddDtoCategory model);

        public Task<DtoResponse<DtoCategory>> UpdateCategoryAsync(DtoCategoryUpdate model);

        public Task<DtoResponse<bool>> DeleteCategoryAsync(int CategoryId);

        public Task<PageResult<DtoCategory>> GetAllAsync(CategoryQuery query);

        public Task<List<DtoCategory>> GetTreeAsync();


        public List<DtoCategory> BuildTree(List<DtoCategory> allcategory, int? parentid, int depth = 0);



    }
}
