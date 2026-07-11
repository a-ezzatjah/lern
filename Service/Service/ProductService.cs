
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
            
           
            product.CreatedAt = DateTime.UtcNow;
            product.ProductCategories = model.CategoryIds
                .Distinct()
                .Select(categoryId => new ProductCategory { Product = product, CategoryId = categoryId })
                .ToList();
            product.SaleOptions = _mapper.Map<List<ProductSaleOption>>(model.SaleOptions);

            _shopDbContext.Products.Add(product);
            await _shopDbContext.SaveChangesAsync();

            var result = await _shopDbContext.Products.Where(x => x.Id == product.Id)
                .ProjectTo<DtoProductAdminList>(_mapper.ConfigurationProvider)
                .FirstOrDefaultAsync();

            return result != null
      ? DtoResponse<DtoProductAdminList>.Success(result)
      : DtoResponse<DtoProductAdminList>.Fail("خطا در بازخوانی اطلاعات...");



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

var product = await _shopDbContext.Products
                .Include(x=>x.ProductCategories)
                .Include(x => x.SaleOptions)
                    .ThenInclude(x => x.SaleOptionColors)
                .FirstOrDefaultAsync(x=>x.Id == model.Id);
            if (product == null)
            {
                return DtoResponse<DtoProductAdminList>.Fail("محصول موجود نمیباشد");
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
            product.SaleOptions = _mapper.Map<List<ProductSaleOption>>(model.SaleOptions);


            product.UpdatedAt = DateTime.UtcNow;
            await _shopDbContext.SaveChangesAsync();

            var result = await _shopDbContext.Products.Where(x => x.Id == product.Id)
                .ProjectTo<DtoProductAdminList>(_mapper.ConfigurationProvider)
                .FirstOrDefaultAsync();

            return result != null
                ? DtoResponse<DtoProductAdminList>.Success(result)
                : DtoResponse<DtoProductAdminList>.Fail("خطا در بازخوانی اطلاعات...");
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

















        public async Task<DtoProductAdminList?> GetAdminByIdAsync(int productId)
        {
            return await _shopDbContext.Products
                .AsNoTracking()
                .Include(x => x.ProductCategories)
                    .ThenInclude(x => x.Category)
                .Include(x => x.SaleOptions)
                    .ThenInclude(x => x.SaleOptionColors)
                .Where(x => x.Id == productId)
                .ProjectTo<DtoProductAdminList>(_mapper.ConfigurationProvider)
                .FirstOrDefaultAsync();
        }

















        public async Task<DtoProductDetail?> GetByIdAsync(int productId)
        {
            return await _shopDbContext.Products
                .AsNoTracking()
                .Include(x => x.ProductCategories)
                    .ThenInclude(x => x.Category)
                .Include(x => x.SaleOptions)
                    .ThenInclude(x => x.SaleOptionColors)
                .Where(x => x.Id == productId)
                .ProjectTo<DtoProductDetail>(_mapper.ConfigurationProvider)
                .FirstOrDefaultAsync();
        }





        public Task<Product?> GetEntityByIdAsync(int productId)
        {
            return _shopDbContext.Products.FirstOrDefaultAsync(x => x.Id == productId);
        }
























        public async Task<PageResult<DtoProductAdminList>> GetFilterAsync(ProductQuery query)
        {
            IQueryable<Product> productQuery = _shopDbContext.Products.OrderByDescending(x => x.Id).AsNoTracking();

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
                else if(query.SearchType == EnumProductSearchType.Color)
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
                .Select(x => new DtoProductAdminList
                {
                    Id = x.Id,
                    Name = x.Name,
                    Slug = x.Slug,
                    CategoriesName = x.ProductCategories.Select(pc => pc.Category.Name).ToList(),
                    SaleOptionTitle = x.SaleOptions.Select(so => so.Title).ToList(),
                    CategoriesCount = x.ProductCategories.Count,
                    SaleOptionsCount = x.SaleOptions.Count,
                    IsActive = x.IsActive,
                    DiscountValue = x.DiscountValue,
                    DiscountType = x.DiscountType,
                    HasDiscount = x.DiscountValue.HasValue && x.DiscountType.HasValue,
                    CreatedAt = x.CreatedAt,
                    UpdatedAt = x.UpdatedAt,
                    SaleOptions = x.SaleOptions.Select(s => new DtoProductAdminSaleOption
                    {
                        Id = s.Id,
                        Title = s.Title,
                        BasePrice = s.BasePrice,
                        ImageUrl = s.ImageUrl,
                        Colors = s.SaleOptionColors.Select(c => new DtoProductAdminSaleOptionColor
                        {
                            Id = c.Id,
                            Color = c.Color,
                            HexCode = c.HexCode,
                            ImageUrl = c.ImageUrl,
                            Price = c.Price
                        }).ToList()
                    }).ToList()
                   
                     
                    
                }).ToListAsync();
                
              
                



            return new PageResult<DtoProductAdminList>
            {
                Items = items,
                TotalCount = totalCount,
                Page = query.Page,
                PageSize = query.PageSize
            };

        }












    }
}
