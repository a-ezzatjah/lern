using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ServiceContract.Common;
using ServiceContract.DTO.DtoCategory;
using ServiceContract.DTO.DtoCategoryView;
using ServiceContract.DTO.DtoCommit;
using ServiceContract.Queries;

namespace ServiceContract.Interfaces
{
    public interface ICategoryService
    {

        public Task<DtoResponse<DtoCategoryAdminList>> AddCategoryAsync(AddDtoCategory model);

        public Task<DtoResponse<DtoCategoryAdminList>> UpdateCategoryAsync(DtoCategoryUpdate model);

        public Task<DtoResponse<bool>> DeleteCategoryAsync(int CategoryId);

        public Task<PageResult<DtoCategoryAdminList>> GetAllAsync(CategoryQuery query);

        public Task<List<DtoCategoryView>> GetTreeAsync();


        public List<DtoCategoryView> BuildTree(List<DtoCategoryAdminList> allcategory, int? parentid, int depth = 0);



    }
}
