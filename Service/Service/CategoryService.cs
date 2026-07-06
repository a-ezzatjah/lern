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
using ServiceContract.DTO.DtoCategoryView;
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
        private const string CategoriesTreeCacheKey = "categories_tree";



        public CategoryService(ShopDbContext shopDbContext, IMapper mapper, IValidator<AddDtoCategory> addDtoCategory
            , IValidator<DtoCategoryUpdate> dtoCategoryUpdate, IMemoryCache memoryCache)
        {

            _shopDbContext = shopDbContext;
            _mapper = mapper;
            _addvalidation = addDtoCategory;
            _updatevalidation = dtoCategoryUpdate;
            _cache = memoryCache;
        }




















        public async Task<DtoResponse<DtoCategoryAdminList>> AddCategoryAsync(AddDtoCategory model)
        {

            if (model == null)
            {
                return DtoResponse<DtoCategoryAdminList>.Fail("دسته بندی ساخته نشده است");
            }
            ;

            var validation = await _addvalidation.ValidateAsync(model);
            if (!validation.IsValid)
            {

                var error = validation.Errors.Select(x => x.ErrorMessage).ToList();
                return DtoResponse<DtoCategoryAdminList>.Fail(error);

            }


            var categorise = _mapper.Map<Category>(model);

            _shopDbContext.Categories.Add(categorise);

            await _shopDbContext.SaveChangesAsync();

            var result = await _shopDbContext.Categories.Where(x => x.Id == categorise.Id)
                 .Select(x => new DtoCategoryAdminList
                 {
                     Id = x.Id,
                     Name = x.Name,
                     Slug = x.Slug,
                     ParentId = x.ParentId,
                     SortOrder = x.SortOrder,
                     ParentName = x.Parent != null ? x.Parent.Name : null,
                     CildrenCount = x.Children.Count(),
                 }
                 ).FirstOrDefaultAsync();

            _cache.Remove(CategoriesTreeCacheKey);

            if (result == null)
            {
                return DtoResponse<DtoCategoryAdminList>.Fail("دسته‌بندی ساخته شد ولی بازیابی نتیجه ناموفق بود");
            }


            return DtoResponse<DtoCategoryAdminList>.Success(result);


        }
























        public async Task<DtoResponse<DtoCategoryAdminList>> UpdateCategoryAsync(DtoCategoryUpdate model)
        {

            if (model == null)
            {
                return DtoResponse<DtoCategoryAdminList>.Fail("مقدار وجود ندارد");
            }

            var validation = await _updatevalidation.ValidateAsync(model);
            if (!validation.IsValid)
            {

                var error = validation.Errors.Select(x => x.ErrorMessage).ToList();
                return DtoResponse<DtoCategoryAdminList>.Fail(error);

            }



            var category = await _shopDbContext.Categories.FirstOrDefaultAsync(x => x.Id == model.Id);


            if (category == null)
            {
                return DtoResponse<DtoCategoryAdminList>.Fail("دسته‌بندی پیدا نشد");
            }


            _mapper.Map(model, category);

            await _shopDbContext.SaveChangesAsync();

            _cache.Remove(CategoriesTreeCacheKey);

            var result =  await _shopDbContext.Categories.Where(x=>x.Id == model.Id)
                .Select(x=> new DtoCategoryAdminList {
                    Id = x.Id,
                    Name = x.Name,
                    Slug = x.Slug,
                    ParentId = x.ParentId,
                    SortOrder = x.SortOrder,
                    ParentName = x.Parent != null ? x.Parent.Name : null,
                    CildrenCount = x.Children.Count(),
                }).FirstOrDefaultAsync();

            if (result == null)
            {
                return DtoResponse<DtoCategoryAdminList>.Fail("ویرایش انجام شد ولی بازیابی نتیجه ناموفق بود");
            }

            return DtoResponse<DtoCategoryAdminList>.Success(result);


        }





































        public async Task<DtoResponse<bool>> DeleteCategoryAsync(int CategoryId)
        {


            var Category = await _shopDbContext.Categories.FirstOrDefaultAsync(x => x.Id == CategoryId);


            if (Category == null)
            {
                return DtoResponse<bool>.Fail("دسته‌بندی پیدا نشد");
            }

            var hasChildren = await _shopDbContext.Categories.AnyAsync(x => x.ParentId == CategoryId);
            if (hasChildren)
            {
                return DtoResponse<bool>.Fail("این دسته زیرمجموعه دارد و قابل حذف نیست");

            }

            var hasProducts = await _shopDbContext.ProductCategories.AnyAsync(x => x.CategoryId == CategoryId);

            if (hasProducts)
            {
                return DtoResponse<bool>.Fail("این دسته به محصول متصل است و قابل حذف نیست");
            }



            _shopDbContext.Remove(Category);

            await _shopDbContext.SaveChangesAsync();

            _cache.Remove(CategoriesTreeCacheKey);

            return DtoResponse<bool>.Success();


        }

























        public async Task<PageResult<DtoCategoryAdminList>> GetAllAsync(CategoryQuery query)
        {
            IQueryable<Category> category = _shopDbContext.Categories
                .OrderBy(x => x.SortOrder)
                .ThenBy(x => x.Id)
                .AsNoTracking();

            query ??= new CategoryQuery();


            if (!string.IsNullOrWhiteSpace(query.SearchText))
            {
                category = category.Where(x => x.Name.Contains(query.SearchText));
            }


            var totalcategory = await category.CountAsync();

            category = category.Skip((query.Page - 1) * query.PageSize).Take(query.PageSize);

            var item = await category.ProjectTo<DtoCategoryAdminList>(_mapper.ConfigurationProvider).ToListAsync();

            return new PageResult<DtoCategoryAdminList>()
            {
                Items = item,
                TotalCount = totalcategory,
                Page = query.Page,
                PageSize = query.PageSize
            };


        }





        public async Task<List<DtoCategoryView>> GetTreeAsync()
        {

            return await _cache.GetOrCreateAsync(CategoriesTreeCacheKey, async entry =>
            {
                entry.AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(30);

                var all = await _shopDbContext.Categories.AsNoTracking()
                    .ProjectTo<DtoCategoryView>(_mapper.ConfigurationProvider).ToListAsync();
                return BuildTree(all, null);
            }) ?? new List<DtoCategoryView>();



        }



        private List<DtoCategoryView> BuildTree(List<DtoCategoryView> allcategory, int? parentid, int depth = 0)
        {

            if (depth > 20) return new List<DtoCategoryView>();

            return allcategory.Where(x => x.ParentId == parentid)
                              .OrderBy(x => x.SortOrder)
                              .Select(x => new DtoCategoryView
                              {
                                  Id = x.Id,
                                  Name = x.Name,
                                  ParentId = x.ParentId,
                                  Slug = x.Slug,
                                  SortOrder = x.SortOrder,
                                  Children = BuildTree(allcategory, x.Id, depth + 1)
                              }).ToList();

        }






    }
}
