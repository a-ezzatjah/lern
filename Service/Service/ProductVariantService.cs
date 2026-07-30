using AutoMapper;
using AutoMapper.QueryableExtensions;
using Entities;
using Microsoft.EntityFrameworkCore;
using ServiceContract.DTO.DtoCommit;
using ServiceContract.DTO.DtoProductVariant;
using ServiceContract.Interfaces;

namespace Service.Service
{
    public class ProductVariantService : IProductVariantService
    {
        private readonly ShopDbContext _shopDbContext;
        private readonly IMapper _mapper;

        public ProductVariantService(ShopDbContext shopDbContext, IMapper mapper)
        {
            _shopDbContext = shopDbContext;
            _mapper = mapper;
        }

        public async Task<ServiceResponseDto<ProductVariantDetailDto>> AddProductVariantAsync(ProductVariantCreateDto model)
        {

            if (model == null)
            {
               return  ServiceResponseDto<ProductVariantDetailDto>.Fail(" موارد به درستی وارد نشده است");
            }

           var saleoption = _shopDbContext.ProductSaleOptions.FirstOrDefault(x => x.Id == model.ProductSaleOptionId);

            if(saleoption == null || saleoption.ProductId != model.ProductSaleOptionId)
            {
                return ServiceResponseDto<ProductVariantDetailDto>.Fail("گزینه فروش مورد نظر یافت نشد");
            }

          var Color = _shopDbContext.ProductSaleOptionColors.FirstOrDefault(x => x.Id == model.ProductSaleOptionColorId);

          if(Color == null || Color.ProductSaleOptionId != model.ProductSaleOptionId)
            {
                return ServiceResponseDto<ProductVariantDetailDto>.Fail("رنگ مورد نظر یافت نشد");
            }


            var variant = new ProductVariant();

            variant.Sku = model.Sku;
            variant.ProductSaleOptionId = model.ProductSaleOptionId;
            variant.ProductSaleOptionColorId = model.ProductSaleOptionColorId;
            variant.Price = model.Price;
            variant.DiscountValue = model.DiscountValue;
            variant.DisconType = model.DisconType;
            variant.DiscountStartAt = model.DiscountStartAt;
            variant.DiscountEndAt = model.DiscountEndAt;
            variant.StockQuantity = model.StockQuantity;
            variant.ReservedQuantity = model.ReservedQuantity;

            _shopDbContext.ProductVariants.Add(variant);
            await _shopDbContext.SaveChangesAsync();

            var result = new ProductVariantDetailDto
            {
                Id = variant.Id,
                Sku = variant.Sku,
                ProductSaleOptionId = variant.ProductSaleOptionId,
                ProductSaleOptionColorId = variant.ProductSaleOptionColorId,
                Price = variant.Price,
                DiscountValue = variant.DiscountValue,
                DisconType = variant.DisconType,
                DiscountStartAt = variant.DiscountStartAt,
                DiscountEndAt = variant.DiscountEndAt,
                StockQuantity = variant.StockQuantity,
                ReservedQuantity = variant.ReservedQuantity
            };

           return ServiceResponseDto<ProductVariantDetailDto>.Success(result); 


        }

        public Task<ServiceResponseDto<ProductVariantDetailDto>> GetProductVariantByIdAsync(int id)
        {
                        throw new NotImplementedException();

        }

        public async Task<ProductVariantUpdateDto?> GetProductVariantForUpdateAsync(int productVariantId)
        {
            return await _shopDbContext.ProductVariants
                .AsNoTracking()
                .Where(x => x.Id == productVariantId)
                .ProjectTo<ProductVariantUpdateDto>(_mapper.ConfigurationProvider)
                .FirstOrDefaultAsync();
        }

        public Task<ServiceResponseDto<IReadOnlyList<ProductVariantListItemDto>>> GetProductVariantsByProductIdAsync(
            int productId)
        {
            throw new NotImplementedException();
        }

        public Task<ServiceResponseDto<ProductVariantDetailDto>> UpdateProductVariantAsync(ProductVariantUpdateDto model)
        {
            throw new NotImplementedException();
        }

        public Task<ServiceResponseDto<bool>> DeleteProductVariantAsync(int id)
        {
            throw new NotImplementedException();
        }
    }
}
