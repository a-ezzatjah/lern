
using System.ComponentModel;
using System.Diagnostics;
using System.Drawing;
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
using ServiceContract.DTO.DtoProductSaleOption;
using ServiceContract.DTO.DtoSaleOptionColor;
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
        private readonly IValidator<ProductCreateDto> _validations;
        private readonly IValidator<ProductUpdateDto> _updateValidator;




        public ProductService(
            ShopDbContext shopDbContext,
            IMapper mapper,
            IValidator<ProductCreateDto> validationRules,
            IValidator<ProductUpdateDto> updateValidator)
        {
            _shopDbContext = shopDbContext;
            _mapper = mapper;
            _validations = validationRules;
            _updateValidator = updateValidator;
        }




        public async Task<ServiceResponseDto<ProductListItemDto>> AddProductAsync(ProductCreateDto model)
        {
            if (model == null)
                return ServiceResponseDto<ProductListItemDto>.Fail("داده نامعتبر است");

            var validationResult = await _validations.ValidateAsync(model);

            if (!validationResult.IsValid)
            {
                var error = validationResult.Errors.Select(x => x.ErrorMessage).ToList();
                return ServiceResponseDto<ProductListItemDto>.Fail(error);
            }

            var normalizedName = model.Name.Trim();
            var productExists = await _shopDbContext.Products.AnyAsync(x => x.Name.ToLower() == normalizedName.ToLower());
            if (productExists)
            {
                return ServiceResponseDto<ProductListItemDto>.Fail("محصول تکراری می‌باشد");
            }

            var product = _mapper.Map<Product>(model);


            product.CreatedAt = DateTime.UtcNow;
            product.ProductCategories = model.CategoryIds
                .Distinct()
                .Select(categoryId => new ProductCategory { Product = product, CategoryId = categoryId })
                .ToList();

            _shopDbContext.Products.Add(product);
            await _shopDbContext.SaveChangesAsync();

            var result = await _shopDbContext.Products.Where(x => x.Id == product.Id)
                .ProjectTo<ProductListItemDto>(_mapper.ConfigurationProvider)
                .FirstOrDefaultAsync();

            return result != null
      ? ServiceResponseDto<ProductListItemDto>.Success(result)
      : ServiceResponseDto<ProductListItemDto>.Fail("خطا در بازخوانی اطلاعات...");



        }








        public async Task<ServiceResponseDto<ProductListItemDto>> UpdateAsync(ProductUpdateDto model)
        {
            if (model == null)
                return ServiceResponseDto<ProductListItemDto>.Fail("داده نامعتبر است");

            var validationResult = await _updateValidator.ValidateAsync(model);

            if (!validationResult.IsValid)
            {
                var error = validationResult.Errors.Select(x => x.ErrorMessage).ToList();
                return ServiceResponseDto<ProductListItemDto>.Fail(error);
            }

            var product = await _shopDbContext.Products
                            .Include(x => x.ProductCategories)
                            .Include(x => x.SaleOptions)
                                .ThenInclude(x => x.SaleOptionColors)
                            .FirstOrDefaultAsync(x => x.Id == model.Id);
            if (product == null)
            {
                return ServiceResponseDto<ProductListItemDto>.Fail("محصول موجود نمی‌باشد");
            }

            _mapper.Map(model, product);

            _shopDbContext.ProductCategories.RemoveRange(product.ProductCategories);

            product.ProductCategories = model.CategoryIds
                   .Distinct()
                   .Select(categoryId => new ProductCategory
                   {
                       ProductId = product.Id,
                       CategoryId = categoryId
                   })
                   .ToList();

            _shopDbContext.ProductSaleOptions.RemoveRange(product.SaleOptions);
            product.SaleOptions = model.SaleOptions
                .Select(saleOption =>
                {
                    var entity = _mapper.Map<ProductSaleOption>(saleOption);
                    entity.Id = 0;
                    entity.ProductId = product.Id;
                    entity.SaleOptionColors = saleOption.SaleOptionColors
                        .Select(color =>
                        {
                            var colorEntity = _mapper.Map<SaleOptionColor>(color);
                            colorEntity.Id = 0;
                            return colorEntity;
                        })
                        .ToList();
                    return entity;
                })
                .ToList();

            product.UpdatedAt = DateTime.UtcNow;
            await _shopDbContext.SaveChangesAsync();

            var result = await _shopDbContext.Products.Where(x => x.Id == product.Id)
                .ProjectTo<ProductListItemDto>(_mapper.ConfigurationProvider)
                .FirstOrDefaultAsync();

            return result != null
                ? ServiceResponseDto<ProductListItemDto>.Success(result)
                : ServiceResponseDto<ProductListItemDto>.Fail("خطا در بازخوانی اطلاعات...");
        }


        public async Task<ServiceResponseDto<bool>> DeleteAsync(int productId)
        {
            var product = await GetEntityByIdAsync(productId);
            if (product == null)
            {
                return ServiceResponseDto<bool>.Fail("محصول مورد نظر یافت نشد");
            }

            _shopDbContext.Products.Remove(product);
            await _shopDbContext.SaveChangesAsync();

            return ServiceResponseDto<bool>.Success();
        }

















        public async Task<ProductListItemDto?> GetListItemByIdAsync(int productId)
        {
            return await _shopDbContext.Products
                .AsNoTracking()
                .Include(x => x.ProductCategories)
                    .ThenInclude(x => x.Category)
                .Include(x => x.SaleOptions)
                    .ThenInclude(x => x.SaleOptionColors)
                .Where(x => x.Id == productId)
                .ProjectTo<ProductListItemDto>(_mapper.ConfigurationProvider)
                .FirstOrDefaultAsync();
        }

        public async Task<ProductUpdateDto?> GetForUpdateAsync(int productId)
        {
            return await _shopDbContext.Products
                .AsNoTracking()
                .Where(x => x.Id == productId)
                .ProjectTo<ProductUpdateDto>(_mapper.ConfigurationProvider)
                .FirstOrDefaultAsync();
        }

















        public async Task<ProductDetailDto?> GetByIdAsync(int productId)
        {
            return await _shopDbContext.Products
                .AsNoTracking()
                .Include(x => x.ProductCategories)
                    .ThenInclude(x => x.Category)
                .Include(x => x.SaleOptions)
                    .ThenInclude(x => x.SaleOptionColors)
                .Where(x => x.Id == productId)
                .ProjectTo<ProductDetailDto>(_mapper.ConfigurationProvider)
                .FirstOrDefaultAsync();
        }





        public Task<Product?> GetEntityByIdAsync(int productId)
        {
            return _shopDbContext.Products.FirstOrDefaultAsync(x => x.Id == productId);
        }
























        public async Task<PageResult<ProductListItemDto>> GetFilterAsync(ProductQuery query)
        {
            IQueryable<Product> productQuery = _shopDbContext.Products.OrderByDescending(x => x.Id).AsNoTracking();

            query ??= new ProductQuery();
            query.Page = Math.Max(query.Page, 1);
            query.PageSize = Math.Clamp(query.PageSize, 1, 100);


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
                else if (query.SearchType == EnumProductSearchType.Slug)
                {
                    productQuery = productQuery.Where(x => x.Slug.Contains(query.SearchText));
                }
                else if (query.SearchType == EnumProductSearchType.CategoryName)
                {
                    productQuery = productQuery.Where(x => x.ProductCategories.Any(s => s.Category.Name.Contains(query.SearchText)));
                }
                else if (query.SearchType == EnumProductSearchType.SaleOptionTitle)
                {
                    productQuery = productQuery.Where(x => x.SaleOptions.Any(s => s.Title.Contains(query.SearchText)));
                }
                else if (query.SearchType == EnumProductSearchType.Color)
                {
                    productQuery = productQuery.Where(x => x.SaleOptions.Any(s => s.SaleOptionColors.Any(y => y.Color.Contains(query.SearchText))));
                }
                else if (query.SearchType == EnumProductSearchType.price)
                {
                    if (decimal.TryParse(query.SearchText, out var price))
                    {
                        productQuery = productQuery.Where(x => x.SaleOptions.Any(s => s.SaleOptionColors.Any(y => y.Price == price)));
                    }

                }

            }

            if (query.HasDiscount.HasValue)
            {
                productQuery = query.HasDiscount.Value
                    ? productQuery.Where(x => x.DiscountValue.HasValue && x.DiscountValue.Value > 0)
                    : productQuery.Where(x => !x.DiscountValue.HasValue || x.DiscountValue.Value <= 0);
            }
            if (query.CreatedFrom.HasValue)
            {
                productQuery = productQuery.Where(x => x.CreatedAt >= query.CreatedFrom.Value);
            }

            if (query.CreatedTo.HasValue)
            {
                var createdToExclusive = query.CreatedTo.Value.Date.AddDays(1);
                productQuery = productQuery.Where(x => x.CreatedAt < createdToExclusive);
            }

            productQuery = (query.SortType, query.Order) switch
            {
                (EnumProductSortType.Id, OrderEnum.ASC) => productQuery.OrderBy(x => x.Id),
                (EnumProductSortType.Id, OrderEnum.DESC) => productQuery.OrderByDescending(x => x.Id),
                (EnumProductSortType.slug, OrderEnum.ASC) => productQuery.OrderBy(x => x.Slug).ThenBy(x => x.Id),
                (EnumProductSortType.slug, OrderEnum.DESC) => productQuery.OrderByDescending(x => x.Slug).ThenByDescending(x => x.Id),
                (EnumProductSortType.Name, OrderEnum.ASC) => productQuery.OrderBy(x => x.Name).ThenBy(x => x.Id),
                (EnumProductSortType.Name, OrderEnum.DESC) => productQuery.OrderByDescending(x => x.Name).ThenByDescending(x => x.Id),
                (EnumProductSortType.Price, OrderEnum.ASC) => productQuery.OrderBy(x => x.SaleOptions.Min(s => s.BasePrice)).ThenBy(x => x.Id),
                (EnumProductSortType.Price, OrderEnum.DESC) => productQuery.OrderByDescending(x => x.SaleOptions.Min(s => s.BasePrice)).ThenByDescending(x => x.Id),
                (EnumProductSortType.HasDiscount, OrderEnum.ASC) => productQuery.OrderBy(x => x.DiscountValue.HasValue && x.DiscountValue.Value > 0).ThenBy(x => x.Id),
                (EnumProductSortType.HasDiscount, OrderEnum.DESC) => productQuery.OrderByDescending(x => x.DiscountValue.HasValue && x.DiscountValue.Value > 0).ThenByDescending(x => x.Id),
                (EnumProductSortType.DiscountValue, OrderEnum.ASC) => productQuery.OrderBy(x => x.DiscountValue).ThenBy(x => x.Id),
                (EnumProductSortType.DiscountValue, OrderEnum.DESC) => productQuery.OrderByDescending(x => x.DiscountValue).ThenByDescending(x => x.Id),
                (EnumProductSortType.CategoryName, OrderEnum.ASC) => productQuery.OrderBy(x => x.ProductCategories.Select(pc => pc.Category.Name).FirstOrDefault()).ThenBy(x => x.Id),
                (EnumProductSortType.CategoryName, OrderEnum.DESC) => productQuery.OrderByDescending(x => x.ProductCategories.Select(pc => pc.Category.Name).FirstOrDefault()).ThenByDescending(x => x.Id),
                (EnumProductSortType.SaleOptionTitle, OrderEnum.ASC) => productQuery.OrderBy(x => x.SaleOptions.Select(s => s.Title).FirstOrDefault()).ThenBy(x => x.Id),
                (EnumProductSortType.SaleOptionTitle, OrderEnum.DESC) => productQuery.OrderByDescending(x => x.SaleOptions.Select(s => s.Title).FirstOrDefault()).ThenByDescending(x => x.Id),
                _ => productQuery.OrderBy(x => x.Id)
            };


            var totalCount = await productQuery.CountAsync();

            productQuery = productQuery
                .Skip((query.Page - 1) * query.PageSize)
                .Take(query.PageSize);

            var items = await productQuery
                .ProjectTo<ProductListItemDto>(_mapper.ConfigurationProvider)
                .ToListAsync();




            return new PageResult<ProductListItemDto>
            {
                Items = items,
                TotalCount = totalCount,
                Page = query.Page,
                PageSize = query.PageSize
            };

        }












    }
}
