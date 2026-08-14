
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
            var product = await _shopDbContext.Products
            .FirstOrDefaultAsync(x=>x.Id == productId);
            

            if (product == null)
            {
                return ServiceResponseDto<bool>.Fail("محصول مورد نظر یافت نشد");
            }


          var hasRelatedSaleoption = await _shopDbContext.ProductSaleOptions.AnyAsync(x=>x.ProductId == productId);
           
           if(hasRelatedSaleoption)
            {
                 return ServiceResponseDto<bool>.Fail("محصول دارای زیر مجموعه میباشد");
            }
 
          

          var hasRelatedProductImage = await _shopDbContext.ProductImages.AnyAsync(x=>x.ProductId == productId);
           
           if(hasRelatedProductImage)
            {
                 return ServiceResponseDto<bool>.Fail("محصول دارای زیر مجموعه میباشد");
            }


            _shopDbContext.Products.Remove(product);
            await _shopDbContext.SaveChangesAsync();

            return ServiceResponseDto<bool>.Success();

        }









         // شاید این متد لازم نشه چون متد getfilter هست  
        public async Task<ProductListItemDto?> GetListItemByIdAsync(int productId)
        {
            return await _shopDbContext.Products
                .AsNoTracking()
                .Where(x => x.Id == productId)
                .ProjectTo<ProductListItemDto>(_mapper.ConfigurationProvider)
                .FirstOrDefaultAsync();
        }



        // متد جهت نمایش اطلاعات کامل محصول برای اپدیت ادمین 
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
                .Where(x => x.Id == productId)
                .ProjectTo<ProductDetailDto>(_mapper.ConfigurationProvider)
                .FirstOrDefaultAsync();
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
                    productQuery = productQuery.Where(x => x.SaleOptions.Any(s => s.SaleOptionColors
                        .Any(y => y.Color.Contains(query.SearchText))));
                }
                else if (query.SearchType == EnumProductSearchType.price)
                {
                    if (decimal.TryParse(query.SearchText, out var price))
                    {
                  
                 productQuery = productQuery.Where(p =>
                                 p.SaleOptions.Any(so =>
                                 so.ProductVariants.Any(v => v.Price == price) ||
                                 so.SaleOptionColors.Any(c => c.ProductVariants.Any(v => v.Price == price))));

                    }

                }

            }



            if (query.HasDiscount.HasValue)
            {
                productQuery = query.HasDiscount.Value
                    ? productQuery.Where(x => x.DiscountValue.HasValue && x.DiscountValue.Value > 0 && x.DiscountType.HasValue)
                    : productQuery.Where(x => !x.DiscountValue.HasValue || x.DiscountValue.Value < 0 || !x.DiscountType.HasValue);
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
                (EnumProductSortType.Price, OrderEnum.ASC) =>  productQuery.OrderBy(x=>x.SaleOptions.SelectMany(s=>s.ProductVariants.Select(p=>(decimal?)p.Price).Concat(s.SaleOptionColors.SelectMany(v=>v.ProductVariants.Select(x=>(decimal?)x.Price)))).Min() ?? 0).ThenBy(x=>x.Id),
                (EnumProductSortType.Price, OrderEnum.DESC) => productQuery.OrderByDescending(x=>x.SaleOptions.SelectMany(s=>s.ProductVariants.Select(p=>(decimal?)p.Price).Concat(s.SaleOptionColors.SelectMany(v=>v.ProductVariants.Select(x=>(decimal?)x.Price)))).Min() ?? 0).ThenByDescending(x=>x.Id),
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
 
 

public async Task<List<ProductCardDto>> GetNewestProductCardsAsync(int take = 8)
{
    take = Math.Clamp(take, 1, 50);

    var products = await _shopDbContext.Products
        .Include(p => p.SaleOptions)
            .ThenInclude(so => so.ProductVariants)
        .Include(p => p.SaleOptions)
            .ThenInclude(so => so.SaleOptionColors)
                .ThenInclude(soc => soc.ProductVariants)
        .Include(p => p.ProductImages)
        .AsNoTracking()
        .AsSplitQuery()
        .Where(p => p.IsActive)
        .OrderByDescending(p => p.CreatedAt)
        .Take(take)
        .ToListAsync();

    return products
        .Select(CreateProductCard)
        .ToList();
}









 // 1
 public async Task<List<ProductCardDto>> GetProductCardsAsync()
    {
        var products = await _shopDbContext.Products
            .Include(p => p.SaleOptions)
                .ThenInclude(so => so.ProductVariants)
            .Include(p => p.SaleOptions)
                .ThenInclude(so => so.SaleOptionColors)
                    .ThenInclude(soc => soc.ProductVariants)
            .Include(p => p.ProductImages)
            .AsNoTracking()
            .AsSplitQuery()
            .Where(p => p.IsActive)
            .ToListAsync();

        return products
            .Select(product => CreateProductCard(product))
            .ToList();
    }



public async Task<List<ProductCardDto>> GetDiscountedProductCardsAsync(int take = 8)
{
    take = Math.Clamp(take, 1, 50);
    var now = DateTime.UtcNow;

    var products = await _shopDbContext.Products
        .AsNoTracking()
        .Where(p => p.IsActive && (
            // تخفیف مستقیم محصول
            (p.DiscountValue.HasValue &&
             p.DiscountValue > 0 &&
             (p.DiscountType == DisconTypeEnum.percent ||
              p.DiscountType == DisconTypeEnum.price) &&
             (!p.DiscountStartAt.HasValue || p.DiscountStartAt <= now) &&
             (!p.DiscountEndAt.HasValue || p.DiscountEndAt >= now) &&
             p.SaleOptions.Any(so => so.ProductVariants.Any(v =>
                 v.Price > 0 &&
                 v.StockQuantity > v.ReservedQuantity)))
            ||
            // تخفیف تنوع‌ها
            p.SaleOptions.Any(so => so.ProductVariants.Any(v =>
                v.Price > 0 &&
                v.StockQuantity > v.ReservedQuantity &&
                v.DiscountValue.HasValue &&
                v.DiscountValue > 0 &&
                (v.DisconType == DisconTypeEnum.percent ||
                 v.DisconType == DisconTypeEnum.price) &&
                (!v.DiscountStartAt.HasValue || v.DiscountStartAt <= now) &&
                (!v.DiscountEndAt.HasValue || v.DiscountEndAt >= now)))
        ))
        .OrderByDescending(p => p.CreatedAt)
        .ThenByDescending(p => p.Id)
        .Take(take)
        .Include(p => p.SaleOptions)
            .ThenInclude(so => so.ProductVariants)
        .Include(p => p.SaleOptions)
            .ThenInclude(so => so.SaleOptionColors)
                .ThenInclude(c => c.ProductVariants)
        .Include(p => p.ProductImages)
        .AsSplitQuery()
        .ToListAsync();

    return products
        .Select(product => CreateProductCard(product, now))
        .Where(pc => pc.HasDiscount)
        .ToList();
}




//2
private static ProductCardDto CreateProductCard(Product product)
{
    return CreateProductCard(product, DateTime.UtcNow);
}

private static ProductCardDto CreateProductCard(Product product, DateTime now)
{
    var variants = product.SaleOptions
        .SelectMany(saleOption => saleOption.ProductVariants)
        .Concat(
            product.SaleOptions
                .SelectMany(saleOption => saleOption.SaleOptionColors)
                .SelectMany(color => color.ProductVariants)
        )
        .DistinctBy(variant => variant.Id)
        .ToList();

var availableVariants = variants // 1
    .Where(variant => variant.AvailableQuantity > 0)
    .ToList();

var isAvailable = availableVariants.Count > 0; // 2 
var hasMultipleVariants = variants.Count > 1;



     var productCard =  new ProductCardDto
    {
        Id = product.Id,
        Name = product.Name,
        Slug = product.Slug,
        PrimaryImageUrl = product.ProductImages?.FirstOrDefault(x => x.IsPrimary)?.ImageUrl
                      ?? product.ProductImages?.FirstOrDefault()?.ImageUrl
                      ?? "/images/default.jpg", // عکس پیش‌فرض
        IsAvailable = isAvailable,
        HasMultipleOptions = hasMultipleVariants,
        HasDiscount = false


    };

 if (!isAvailable)
    {
        return productCard;
    }

    var pricedVariants = availableVariants
        .Select(variant => new
        {
            OriginalPrice = variant.Price,
            FinalPrice = CalculateVariantPrice(variant, product, now)
        })
        .ToList();

    productCard.HasDiscount = pricedVariants
        .Any(item => item.FinalPrice < item.OriginalPrice);

    productCard.DiscountPercent = pricedVariants
        .Where(item =>
            item.OriginalPrice > 0 &&
            item.FinalPrice < item.OriginalPrice)
        .Select(item => Math.Round(
            (item.OriginalPrice - item.FinalPrice) * 100m / item.OriginalPrice,
            2))
        .DefaultIfEmpty()
        .Max();

    if (!productCard.HasDiscount)
    {
        productCard.DiscountPercent = null;
    }

    if (hasMultipleVariants)
    {
        productCard.MinPrice = pricedVariants
            .Min(item => item.FinalPrice);
    }
    else
    {
        productCard.FinalPrice = pricedVariants
            .Single()
            .FinalPrice;
    }

    return productCard ;


}




//3
private static decimal ApplyDiscount(
    decimal basePrice,
    decimal? discountValue,
    DisconTypeEnum? discountType,
    DateTime? start,
    DateTime? end,
    DateTime now)
{
    if (!discountValue.HasValue ||
        !discountType.HasValue ||
        discountValue.Value <= 0 ||
        !IsSupportedDiscountType(discountType.Value))
    {
        return basePrice;
    }

      if (!IsDiscountActive(start, end, now))
        return basePrice;


    var finalPrice = discountType.Value switch
    {
        DisconTypeEnum.percent =>
            basePrice - (basePrice * discountValue.Value / 100m),

        DisconTypeEnum.price =>
            basePrice - discountValue.Value,

        _ => basePrice
    };

    return Math.Max(0, finalPrice);
}



//4
private static decimal CalculateVariantPrice(
    ProductVariant variant,
    Product product,
    DateTime now)
{
    var hasActiveVariantDiscount =
        variant.DiscountValue.HasValue &&
        variant.DisconType.HasValue &&
        variant.DiscountValue.Value > 0 &&
        IsSupportedDiscountType(variant.DisconType.Value) &&
        IsDiscountActive(
            variant.DiscountStartAt,
            variant.DiscountEndAt,
            now);

    if (hasActiveVariantDiscount)
    {
        return ApplyDiscount(
            variant.Price,
            variant.DiscountValue,
            variant.DisconType,
            variant.DiscountStartAt,
            variant.DiscountEndAt,
            now);
    }

    return ApplyDiscount(
        variant.Price,
        product.DiscountValue,
        product.DiscountType,
        product.DiscountStartAt,
        product.DiscountEndAt,
        now);
}



//5
private static bool IsDiscountActive(
    DateTime? discountStartAt,
    DateTime? discountEndAt,
    DateTime now)
{
    if (discountStartAt.HasValue && now < discountStartAt.Value)
    {
        return false;
    }

    if (discountEndAt.HasValue && now > discountEndAt.Value)
    {
        return false;
    }

    return true;
}

private static bool IsSupportedDiscountType(DisconTypeEnum discountType)
{
    return discountType is DisconTypeEnum.percent or DisconTypeEnum.price;
}



 
    }



}






    



