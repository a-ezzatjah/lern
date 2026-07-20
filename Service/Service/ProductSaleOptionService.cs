using AutoMapper;
using Entities;
using Microsoft.EntityFrameworkCore;
using ServiceContract.DTO.DtoCommit;
using ServiceContract.DTO.DtoProductSaleOption;
using ServiceContract.Interfaces;

namespace Service.Service
{
    public class ProductSaleOptionService : IProductSaleOptionService
    {
        private readonly IMapper _mapper;
        private readonly ShopDbContext _shopDbContext;

        public ProductSaleOptionService(ShopDbContext shopDbContext, IMapper mapper)
        {
            _shopDbContext = shopDbContext;
            _mapper = mapper;
        }

















        public async Task<ServiceResponseDto<ProductSaleOptionListItemDto>> AddProductSaleOptionAsync(
            ProductSaleOptionCreateDto model)
        {
            if (model == null)
            {
                return ServiceResponseDto<ProductSaleOptionListItemDto>.Fail("حالت فروش وارد نشده است");
            }

            var productSaleOption = _mapper.Map<ProductSaleOption>(model);

            _shopDbContext.ProductSaleOptions.Add(productSaleOption);
            await _shopDbContext.SaveChangesAsync();

            var result = _mapper.Map<ProductSaleOptionListItemDto>(productSaleOption);
            return ServiceResponseDto<ProductSaleOptionListItemDto>.Success(result);
        }















        public async Task<ServiceResponseDto<ProductSaleOptionListItemDto>> GetProductSaleOptionByIdAsync(int id)
        {
            var productSaleOption = await _shopDbContext.ProductSaleOptions
                .AsNoTracking()
                .FirstOrDefaultAsync(x => x.Id == id);

            if (productSaleOption == null)
            {
                return ServiceResponseDto<ProductSaleOptionListItemDto>.Fail("گزینه فروش موجود نمی‌باشد");
            }

            var result = _mapper.Map<ProductSaleOptionListItemDto>(productSaleOption);
            return ServiceResponseDto<ProductSaleOptionListItemDto>.Success(result);
        }










        public async Task<ServiceResponseDto<bool>> DeleteProductSaleOptionAsync(int id)
        {
            var productSaleOption = await _shopDbContext.ProductSaleOptions
                .FirstOrDefaultAsync(x => x.Id == id);

            if (productSaleOption == null)
            {
                return ServiceResponseDto<bool>.Fail("گزینه فروش موجود نمی‌باشد");
            }

            _shopDbContext.ProductSaleOptions.Remove(productSaleOption);
            await _shopDbContext.SaveChangesAsync();

            return ServiceResponseDto<bool>.Success(true);
        }

















        public async Task<ServiceResponseDto<ProductSaleOptionListItemDto>> UpdateProductSaleOptionAsync(
            ProductSaleOptionUpdateDto model)
        {
            if (model == null)
            {
                return ServiceResponseDto<ProductSaleOptionListItemDto>.Fail("اطلاعات ویرایش وارد نشده است");
            }

            if (model.Id < 1)
            {
                return ServiceResponseDto<ProductSaleOptionListItemDto>.Fail("شناسه گزینه فروش معتبر نیست");
            }

            var productSaleOption = await _shopDbContext.ProductSaleOptions
                .FirstOrDefaultAsync(x => x.Id == model.Id);

            if (productSaleOption == null)
            {
                return ServiceResponseDto<ProductSaleOptionListItemDto>.Fail("گزینه فروش موجود نمی‌باشد");
            }

            _mapper.Map(model, productSaleOption);

          

            await _shopDbContext.SaveChangesAsync();

            var result = _mapper.Map<ProductSaleOptionListItemDto>(productSaleOption);
            return ServiceResponseDto<ProductSaleOptionListItemDto>.Success(result);
        }
    }
}
