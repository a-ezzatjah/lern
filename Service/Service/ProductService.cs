
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




        public async Task<DtoResponse<DtoProduct>> AddProductAsync(DtoproductAdd model)
        {
            if (model == null)
                return DtoResponse<DtoProduct>.Fail("داده نامعتبر است");

            var validationResult = await _validations.ValidateAsync(model);

            if (!validationResult.IsValid)
            {
                var error = validationResult.Errors.Select(x => x.ErrorMessage).ToList();
                return DtoResponse<DtoProduct>.Fail(error);
            }

            var productExists = await _shopDbContext.Products.AnyAsync(x => x.Name.ToLower() == model.Name.ToLower());
            if (productExists)
            {
                return DtoResponse<DtoProduct>.Fail("محصول تکراری میباشد");
            }

            var product = _mapper.Map<Product>(model);

            _shopDbContext.Products.Add(product);
            await _shopDbContext.SaveChangesAsync();

            var result = _mapper.Map<DtoProduct>(product);

            return DtoResponse<DtoProduct>.Success(result);
        }








        public async Task<DtoResponse<DtoProduct>> UpdateAsync(DtoProductUpdate model)
        {
            if (model == null)
                return DtoResponse<DtoProduct>.Fail("داده نامعتبر است");

            var validationResult = await _updateValidator.ValidateAsync(model);

            if (!validationResult.IsValid)
            {
                var error = validationResult.Errors.Select(x => x.ErrorMessage).ToList();
                return DtoResponse<DtoProduct>.Fail(error);
            }

            var product = await GetEntityByIdAsync(model.Id.Value);
            if (product == null)
            {
                return DtoResponse<DtoProduct>.Fail("محصول موجود نمیباشد");
            }

            _mapper.Map(model, product);
            await _shopDbContext.SaveChangesAsync();

            var result = _mapper.Map<DtoProduct>(product);

            return DtoResponse<DtoProduct>.Success(result);
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



































        public async Task<DtoProduct?> GetByIdAsync(int productId)
        {
            return await _shopDbContext.Products
                .Where(x => x.Id == productId)
                .ProjectTo<DtoProduct>(_mapper.ConfigurationProvider)
                .FirstOrDefaultAsync();
        }





        public Task<Product?> GetEntityByIdAsync(int productId)
        {
            return _shopDbContext.Products.FirstOrDefaultAsync(x => x.Id == productId);
        }
























        public async Task<PageResult<DtoProduct>> GetFilterAsync(ProductQuery query)
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
                else if (
                    query.SearchType == EnumProductSearchType.Price
                    && decimal.TryParse(query.SearchText, out var priceValue))
                {
                    productQuery = productQuery.Where(x => x.Price == priceValue);
                }
            }

            productQuery = (query.SortType, query.Order) switch
            {
                (EnumProductSortType.Name, OrderEnum.ASC) => productQuery.OrderBy(x => x.Name),
                (EnumProductSortType.Name, OrderEnum.DESC) => productQuery.OrderByDescending(x => x.Name),
                (EnumProductSortType.Price, OrderEnum.ASC) => productQuery.OrderBy(x => x.Price),
                (EnumProductSortType.Price, OrderEnum.DESC) => productQuery.OrderByDescending(x => x.Price),
                _ => productQuery.OrderBy(x => x.Id)
            };


            var totalCount = await productQuery.CountAsync();

            productQuery = productQuery
                .Skip((query.Page - 1) * query.PageSize)
                .Take(query.PageSize);

            var items = await productQuery
                .ProjectTo<DtoProduct>(_mapper.ConfigurationProvider)
                .ToListAsync();



            return new PageResult<DtoProduct>
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
                new DtoSearchOption { Key = nameof(DtoProduct.Name), Title = "نام" },
                new DtoSearchOption { Key = nameof(DtoProduct.Description), Title = "توضیحات" },
                new DtoSearchOption { Key = nameof(DtoProduct.Price), Title = "قیمت" }
            };

            return Task.FromResult(result);
        }








    }
}
