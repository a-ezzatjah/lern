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
            
            var product = await _shopDbContext.Products.FirstOrDefaultAsync(x=>x.Id == model.ProductId);
            if(product == null|| product.Id != model.ProductId)
            {
                return ServiceResponseDto<ProductSaleOptionListItemDto>.Fail("محصول مورد نظر یافت نشد");
            }
           
           var Dublicate = await _shopDbContext.ProductSaleOptions.AnyAsync(x=>x.ProductId == model.ProductId && x.Title == model.Title);
            if(Dublicate)
            {
                return ServiceResponseDto<ProductSaleOptionListItemDto>.Fail("گزینه فروش وارد شده تکراری است");
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

          var hasRelatedSaleOption = await _shopDbContext.ProductVariants.AnyAsync(x => x.ProductSaleOptionId == id);
            if (hasRelatedSaleOption)
            {
                return ServiceResponseDto<bool>.Fail("گزینه فروش دارای واریانت مرتبط است و نمی‌توان آن را حذف کرد");
            }

        var hasRelatedColor = await _shopDbContext.ProductSaleOptionColors.AnyAsync(x => x.ProductSaleOptionId == id);
            if (hasRelatedColor)
            {
                return ServiceResponseDto<bool>.Fail("گزینه فروش دارای رنگ مرتبط است و نمی‌توان آن را حذف کرد");
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

            if(productSaleOption.ProductId != model.ProductId)
            {
                
                var exists = await _shopDbContext.Products.AnyAsync(x=>x.Id == model.ProductId);

                if(!exists)
                {
                    return ServiceResponseDto<ProductSaleOptionListItemDto>.Fail("محصول موجود نمیباشد است");
                }

               var Dependencies = await _shopDbContext.ProductVariants.AnyAsync(x=>x.ProductSaleOptionId == productSaleOption.Id)||
                                  await _shopDbContext.ProductSaleOptionColors.AnyAsync(x=>x.ProductSaleOptionId == productSaleOption.Id);

                if(Dependencies)
                {
                    return ServiceResponseDto<ProductSaleOptionListItemDto>.Fail("گزینه فروش دارای واریانت مرتبط است و نمی‌توان آن را تغییر داد");
                }
           

            }
            

            _mapper.Map(model, productSaleOption);

          

            await _shopDbContext.SaveChangesAsync();

            var result = _mapper.Map<ProductSaleOptionListItemDto>(productSaleOption);
            return ServiceResponseDto<ProductSaleOptionListItemDto>.Success(result);
        }
    }
}
