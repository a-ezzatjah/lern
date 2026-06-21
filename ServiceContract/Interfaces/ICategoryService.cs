using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ServiceContract.DTO.DtoCategory;
using ServiceContract.DTO.DtoCommit;
using ServiceContract.Queries;

namespace ServiceContract.Interfaces
{
    public interface ICategoryService
    {

        public Task<DtoResponse<DtoCategory>> AddCategoryAsync(AddDtoCategory model);

        public Task<DtoResponse<DtoCategory>> UpdateCategoryAsync(DtoCategoryUpdate model);

        public DtoResponse<bool> DeleteCategoryAsync(int Categotyid);

        public Task<List<DtoCategory>> GetAllAsync(CategoryQuery query);

        public List<DtoCategory> BuildTree(List<DtoCategory> allcategory, int? parentid);



    }
}
