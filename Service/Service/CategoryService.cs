using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AutoMapper;
using AutoMapper.QueryableExtensions;
using Entities;
using FluentValidation;
using FluentValidation.Validators;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;
using ServiceContract.Common;
using ServiceContract.DTO.DtoCategory;
using ServiceContract.DTO.DtoCommit;
using ServiceContract.Interfaces;
using ServiceContract.Queries;

namespace Service.Service
{
    public class CategoryService : ICategoryService
    {
        private readonly ShopDbContext _shopDbContext;
        private readonly IMapper _mapper;
        private readonly IValidator<AddDtoCategory> _addvalidation;
        private readonly IValidator<DtoCategoryUpdate> _updatevalidation;
        private readonly IMemoryCache _cache;
        


        public CategoryService(ShopDbContext shopDbContext, IMapper mapper, IValidator<AddDtoCategory> addDtoCategory
            , IValidator<DtoCategoryUpdate> dtoCategoryUpdate,IMemoryCache memoryCache)
        {

            _shopDbContext = shopDbContext;
            _mapper = mapper;
            _addvalidation = addDtoCategory;
            _updatevalidation = dtoCategoryUpdate;
            _cache = memoryCache;
        }





        public async Task<DtoResponse<DtoCategory>> AddCategoryAsync(AddDtoCategory model)
        {

            if (model == null)
            {
                return DtoResponse<DtoCategory>.Fail("دسته بندی ساخته نشده است");
            }
            ;

            var validation = _addvalidation.Validate(model);
            if (!validation.IsValid)
            {

                var error = validation.Errors.Select(x => x.ErrorMessage).ToList();
                return DtoResponse<DtoCategory>.Fail(error);

            }


            var categorise = _mapper.Map<Category>(model);

            _shopDbContext.categories.Add(categorise);

            await _shopDbContext.SaveChangesAsync();

            var result = _mapper.Map<DtoCategory>(categorise);


            return DtoResponse<DtoCategory>.Success(result);


        }

        public DtoResponse<bool> DeleteCategoryAsync(int Categotyid)
        {
            throw new NotImplementedException();
        }


        public async Task<PageResult<DtoCategory>> GetAllAsync(CategoryQuery query)
        {
            var category = _shopDbContext.categories.AsNoTracking();

            if (!string.IsNullOrWhiteSpace(query.SearchText))
            {
                category = category.Where(x => x.Name.Contains(query.SearchText));
            }


            var totalcategory = await category.CountAsync();

            category =  category.Skip((query.Page - 1) * query.PageSize).Take(query.PageSize);

            var item = await category.ProjectTo<DtoCategory>(_mapper.ConfigurationProvider).ToListAsync();

            return new PageResult<DtoCategory>()
            {
                Items = item,
                TotalCount = totalcategory,
                Page=query.Page,
                PageSize=query.PageSize

            };


        }

        



        public async Task<List<DtoCategory>> GetTreeAsync()
        {

            return await _cache.GetOrCreateAsync("categories_tree", async entry =>
            {
                var all = await _shopDbContext.categories.AsNoTracking()
                    .ProjectTo<DtoCategory>(_mapper.ConfigurationProvider).ToListAsync();
                return BuildTree(all, null);
            });

          

        }



        public  List<DtoCategory> BuildTree(List<DtoCategory> allcategory, int? parentid)
        {

            return  allcategory.Where(x => x.ParentId == parentid)
                              .OrderBy(x => x.SortOrder)
                              .Select(x => new DtoCategory
                              {
                                  Id = x.Id,
                                  Name = x.Name,
                                  ParentId = x.ParentId,
                                  Slug = x.Slug,
                                  SortOrder = x.SortOrder,
                                  Children = BuildTree(allcategory, x.Id)
                              }).ToList();

        }

        public Task<DtoResponse<DtoCategory>> UpdateCategoryAsync(DtoCategoryUpdate model)
        {
            throw new NotImplementedException();
        }
    }
}
