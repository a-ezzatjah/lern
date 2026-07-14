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

        public Task<ServiceResponseDto<CategoryAdminListItemDto>> AddCategoryAsync(CategoryCreateDto model);

        public Task<ServiceResponseDto<CategoryAdminListItemDto>> UpdateCategoryAsync(CategoryPatchFieldDto model);

        public Task<ServiceResponseDto<bool>> DeleteCategoryAsync(int CategoryId);

        public Task<PageResult<CategoryAdminListItemDto>> GetAllAsync(CategoryQuery query);

        public Task<List<CategoryTreeItemDto>> GetTreeAsync();





    }
}
