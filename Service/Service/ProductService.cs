
using System.ComponentModel;
using System.Linq.Expressions;
using AutoMapper;
using AutoMapper.QueryableExtensions;
using DTO;
using Entities;
using FluentValidation;
using Microsoft.EntityFrameworkCore;
using Service.Mapping;
using Service.Validators;
using ServiceContract.Common;
using ServiceContract.DTO.DtoCommit;
using ServiceContract.DTO.DtoProduct;
using ServiceContract.Enums;
using ServiceContract.Interfaces;
using ServiceContract.Quaries;
using ServiceContract.Queries;

namespace Service.Service
{
    public class ProductService : IProductService
    {
        private readonly IMapper _mapper;
        private readonly ShopDbContext _shopDbContext;
        private readonly IValidator<DtoproductAdd> _validations;
        private readonly IValidator<DtoProductUpdate> _updateValidator;




        public ProductService(
            ShopDbContext shopDbContext,
            IMapper mapper,
            IValidator<DtoproductAdd> validationRules,
            IValidator<DtoProductUpdate> updateValidator)
        {
            _shopDbContext = shopDbContext;
            _mapper = mapper;
            _validations = validationRules;
            _updateValidator = updateValidator;
        }




        public async Task<DtoResponse<DtoProductAdminList>> AddProductAsync(DtoproductAdd model)
        {
            if (model == null)
                return DtoResponse<DtoProductAdminList>.Fail("داده نامعتبر است");

            var validationResult = await _validations.ValidateAsync(model);

            if (!validationResult.IsValid)
            {
                var error = validationResult.Errors.Select(x => x.ErrorMessage).ToList();
                return DtoResponse<DtoProductAdminList>.Fail(error);
            }

            var normalizedName = model.Name.Trim();
            var productExists = await _shopDbContext.Products.AnyAsync(x => x.Name.ToLower() == normalizedName.ToLower());
            if (productExists)
            {
                return DtoResponse<DtoProductAdminList>.Fail("محصول تکراری میباشد");
            }

            var product = _mapper.Map<Product>(model);
            product.Name = model.Name.Trim();
            product.Slug = model.Slug.Trim();
            product.CreatedAt = DateTime.UtcNow;
            product.ProductCategories = model.CategoryIds
                .Distinct()
                .Select(categoryId => new ProductCategory { Product = product, CategoryId = categoryId })
                .ToList();

            _shopDbContext.Products.Add(product);
            await _shopDbContext.SaveChangesAsync();

            var result = _mapper.Map<DtoProductAdminList>(product);

            return DtoResponse<DtoProductAdminList>.Success(result);
        }








        public async Task<DtoResponse<DtoProductAdminList>> UpdateAsync(DtoProductUpdate model)
        {
            if (model == null)
                return DtoResponse<DtoProductAdminList>.Fail("داده نامعتبر است");

            var validationResult = await _updateValidator.ValidateAsync(model);

            if (!validationResult.IsValid)
            {
                var error = validationResult.Errors.Select(x => x.ErrorMessage).ToList();
                return DtoResponse<DtoProductAdminList>.Fail(error);
            }

var product = await GetEntityByIdAsync(model.Id);
            if (product == null)
            {
                return DtoResponse<DtoProductAdminList>.Fail("محصول موجود نمیباشد");
            }

            _mapper.Map(model, product);
            await _shopDbContext.Entry(product).Collection(x => x.ProductCategories).LoadAsync();
            product.ProductCategories.Clear();
            foreach (var categoryId in model.CategoryIds.Distinct())
            {
                product.ProductCategories.Add(new ProductCategory { ProductId = product.Id, CategoryId = categoryId });
            }
            product.UpdatedAt = DateTime.UtcNow;
            await _shopDbContext.SaveChangesAsync();

            var result = _mapper.Map<DtoProductAdminList>(product);

            return DtoResponse<DtoProductAdminList>.Success(result);
        }








        public async Task<DtoResponse<bool>> DeleteAsync(int productId)
        {
            var product = await GetEntityByIdAsync(productId);
            if (product == null)
            {
                return DtoResponse<bool>.Fail("محصول مورد نظر یافت نشد");
            }

            _shopDbContext.Products.Remove(product);
            await _shopDbContext.SaveChangesAsync();

            return DtoResponse<bool>.Success();
        }



































        public async Task<DtoProductAdminList?> GetByIdAsync(int productId)
        {
            return await _shopDbContext.Products
                .AsNoTracking()
                .Where(x => x.Id == productId)
                .ProjectTo<DtoProductAdminList>(_mapper.ConfigurationProvider)
                .FirstOrDefaultAsync();
        }





        public Task<Product?> GetEntityByIdAsync(int productId)
        {
            return _shopDbContext.Products.FirstOrDefaultAsync(x => x.Id == productId);
        }
























        public async Task<PageResult<DtoProductAdminList>> GetFilterAsync(ProductQuery query)
        {
            IQueryable<Product> productQuery = _shopDbContext.Products.AsNoTracking();

            query ??= new ProductQuery();


            if (!string.IsNullOrWhiteSpace(query.SearchText))
            {
                if (query.SearchType == EnumProductSearchType.All)
                {
                    productQuery = productQuery.Where(x =>
                        x.Name.Contains(query.SearchText) ||
                        x.Description != null && x.Description.Contains(query.SearchText));
                }
                else if (query.SearchType == EnumProductSearchType.Name)
                {
                    productQuery = productQuery.Where(x => x.Name.Contains(query.SearchText));
                }
                else if (query.SearchType == EnumProductSearchType.Description)
                {
                    productQuery = productQuery.Where(x =>
                            x.Description != null &&
                            x.Description.Contains(query.SearchText));
                }
            }

            productQuery = (query.SortType, query.Order) switch
            {
                (EnumProductSortType.Name, OrderEnum.ASC) => productQuery.OrderBy(x => x.Name),
                (EnumProductSortType.Name, OrderEnum.DESC) => productQuery.OrderByDescending(x => x.Name),
                _ => productQuery.OrderBy(x => x.Id)
            };


            var totalCount = await productQuery.CountAsync();

            productQuery = productQuery
                .Skip((query.Page - 1) * query.PageSize)
                .Take(query.PageSize);

            var items = await productQuery
                .ProjectTo<DtoProductAdminList>(_mapper.ConfigurationProvider)
                .ToListAsync();



            return new PageResult<DtoProductAdminList>
            {
                Items = items,
                TotalCount = totalCount,
                Page = query.Page,
                PageSize = query.PageSize
            };

        }

















        public Task<List<DtoSearchOption>> GetSelectAsync()
        {
            var result = new List<DtoSearchOption>
            {
                new DtoSearchOption { Key = nameof(DtoProductAdminList.Name), Title = "نام" },
                new DtoSearchOption { Key = nameof(DtoProductAdminList.Description), Title = "توضیحات" }
            };

            return Task.FromResult(result);
        }








    }
}
