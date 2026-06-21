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
using ServiceContract.Common;
using ServiceContract.DTO.DtoCategory;
using ServiceContract.DTO.DtoCommit;
using ServiceContract.Interfaces;
using ServiceContract.Queries;

namespace Service.Service
{
    public class CategoryService : ICategoryService
    {
        public readonly ShopDbContext _shopDbContext;
        public readonly IMapper _mapper;
        public readonly IValidator<AddDtoCategory> _addvalidation;
        public readonly IValidator<DtoCategoryUpdate> _updatevalidation;


        public CategoryService(ShopDbContext shopDbContext, IMapper mapper, IValidator<AddDtoCategory> addDtoCategory
            , IValidator<DtoCategoryUpdate> dtoCategoryUpdate)
        {

            _shopDbContext = shopDbContext;
            _mapper = mapper;
            _addvalidation = addDtoCategory;
            _updatevalidation = dtoCategoryUpdate;

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
            var categoryqury = _shopDbContext.categories.AsNoTracking();



            if (!string.IsNullOrWhiteSpace(query.SearchText))
            {
                categoryqury = categoryqury.Where(x => x.Name.Contains(query.SearchText));
            }


            var allcategory = categoryqury
                .Select(x => new DtoCategory
                {
                    Id = x.Id,
                    Name = x.Name,
                    ParentId = x.ParentId,
                    Slug = x.Slug,
                    SortOrder = x.SortOrder,

                }).ToList();

            var result = BuildTree(allcategory, null);





            var totalcount = allcategory.Count();




            return new PageResult<DtoCategory>
            {
                TotalCount = totalcount,




            };










            
        }



        public List<DtoCategory> BuildTree(List<DtoCategory> allcategory, int? parentid)
        {

            return allcategory.Where(x => x.ParentId == parentid)
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
