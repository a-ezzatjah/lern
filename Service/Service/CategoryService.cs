using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AutoMapper;
using Entities;
using FluentValidation.Validators;
using Microsoft.EntityFrameworkCore;
using ServiceContract.DTO.DtoCategory;
using ServiceContract.DTO.DtoCommit;
using ServiceContract.Interfaces;

namespace Service.Service
{
    public class CategoryService : ICategoryService
    {
        public readonly ShopDbContext _shopDbContext;
        public readonly IMapper _mapper;

        public CategoryService(ShopDbContext shopDbContext,IMapper mapper)
        {

            _shopDbContext = shopDbContext;
            _mapper = mapper;

        }





        public async Task<DtoResponse<DtoCategory>> AddCategoryAsync(AddDtoCategory model)
        {

            if(model == null)
            {
                return DtoResponse<DtoCategory>.Fail("دسته بندی ساخته نشده است");
            };




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

        public async Task<List<DtoCategory>> GetAllAsync()
        {
            var allcategory = await _shopDbContext.categories.AsNoTracking()
                .Select(x => new DtoCategory
                {
                    Id = x.Id,
                    Name = x.Name,
                    ParentId = x.ParentId,
                    Slug = x.Slug,
                    SortOrder = x.SortOrder,

                }).ToListAsync();

            var result = BuildTree(allcategory, null);
            return result;


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
