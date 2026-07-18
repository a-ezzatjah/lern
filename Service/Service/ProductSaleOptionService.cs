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

        public async Task<ServiceResponseDto<ProductSaleOptionAdminDto>> AddProductSaleOptionAsync(
            ProductSaleOptionCreateDto model)
        {
            if (model == null)
            {
                return ServiceResponseDto<ProductSaleOptionAdminDto>.Fail("حالت فروش وارد نشده است");
            }

            var productSaleOption = _mapper.Map<ProductSaleOption>(model);

            _shopDbContext.ProductSaleOptions.Add(productSaleOption);
            await _shopDbContext.SaveChangesAsync();

            var result = _mapper.Map<ProductSaleOptionAdminDto>(productSaleOption);
            return ServiceResponseDto<ProductSaleOptionAdminDto>.Success(result);
        }

        public async Task<ServiceResponseDto<ProductSaleOptionAdminDto>> GetProductSaleOptionByIdAsync(int id)
        {
            var productSaleOption = await _shopDbContext.ProductSaleOptions
                .AsNoTracking()
                .Include(x => x.SaleOptionColors)
                .FirstOrDefaultAsync(x => x.Id == id);

            if (productSaleOption == null)
            {
                return ServiceResponseDto<ProductSaleOptionAdminDto>.Fail("گزینه فروش موجود نمی‌باشد");
            }

            var result = _mapper.Map<ProductSaleOptionAdminDto>(productSaleOption);
            return ServiceResponseDto<ProductSaleOptionAdminDto>.Success(result);
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

        public async Task<ServiceResponseDto<ProductSaleOptionAdminDto>> UpdateProductSaleOptionAsync(
            ProductSaleOptionPatchFieldDto model)
        {
            if (model == null)
            {
                return ServiceResponseDto<ProductSaleOptionAdminDto>.Fail("اطلاعات ویرایش وارد نشده است");
            }

            if (model.Id < 1)
            {
                return ServiceResponseDto<ProductSaleOptionAdminDto>.Fail("شناسه گزینه فروش معتبر نیست");
            }

            var productSaleOption = await _shopDbContext.ProductSaleOptions
                .Include(x => x.SaleOptionColors)
                .FirstOrDefaultAsync(x => x.Id == model.Id);

            if (productSaleOption == null)
            {
                return ServiceResponseDto<ProductSaleOptionAdminDto>.Fail("گزینه فروش موجود نمی‌باشد");
            }

            _mapper.Map(model, productSaleOption);

            if (model.SaleOptionColors != null)
            {
                _shopDbContext.SaleOptionColors.RemoveRange(productSaleOption.SaleOptionColors);
                productSaleOption.SaleOptionColors = model.SaleOptionColors
                    .Select(x => new SaleOptionColor
                    {
                        Color = x.Color,
                        HexCode = x.HexCode,
                        ImageUrl = x.ImageUrl,
                        Price = x.Price
                    })
                    .ToList();
            }

            await _shopDbContext.SaveChangesAsync();

            var result = _mapper.Map<ProductSaleOptionAdminDto>(productSaleOption);
            return ServiceResponseDto<ProductSaleOptionAdminDto>.Success(result);
        }
    }
}
