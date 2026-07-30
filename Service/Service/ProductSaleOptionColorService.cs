using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AutoMapper;
using AutoMapper.QueryableExtensions;
using Entities;
using Microsoft.EntityFrameworkCore;
using ServiceContract.DTO.DtoCommit;
using ServiceContract.DTO.DtoProductSaleOptionColor;
using ServiceContract.Interfaces;

namespace Service.Service
{
    public class ProductSaleOptionColorService : IProductSaleOptionColorService
    {

        private readonly ShopDbContext _shopDbContext;

        private readonly IMapper _mapper;




        public ProductSaleOptionColorService(ShopDbContext shopDbContext , IMapper mapper)
        {

            _shopDbContext = shopDbContext;

            _mapper = mapper;


        }


        public async Task<ProductSaleOptionColorUpdateDto?> GetProductSaleOptionColorForUpdateAsync(
            int productSaleOptionColorId)
        {
            return await _shopDbContext.ProductSaleOptionColors
                .AsNoTracking()
                .Where(x => x.Id == productSaleOptionColorId)
                .ProjectTo<ProductSaleOptionColorUpdateDto>(_mapper.ConfigurationProvider)
                .FirstOrDefaultAsync();
        }










      public ServiceResponseDto<ProductSaleOptionColorDetailDto> CreateProductSaleOptionColor(
          ProductSaleOptionColorCreateDto model)
        {

            if (model == null)
            {
               return ServiceResponseDto<ProductSaleOptionColorDetailDto>.Fail("اطلاعات ساخت موحود نمیباشد");
            }

            var ProductColor = new ProductSaleOptionColor();

            ProductColor.ProductSaleOptionId = model.ProductSaleOptionId;
            ProductColor.Color = model.Color;
            ProductColor.HexCode = model.HexCode;
            ProductColor.Price = model.Price;
            ProductColor.ImageUrl = model.ImageUrl;


            _shopDbContext.ProductSaleOptionColors.Add(ProductColor);
            _shopDbContext.SaveChanges();



            var result = new ProductSaleOptionColorDetailDto();

            
            result.Color = ProductColor.Color;
            result.HexCode = ProductColor.HexCode;
            result.Price = ProductColor.Price;
            result.ImageUrl = ProductColor.ImageUrl;


            return ServiceResponseDto<ProductSaleOptionColorDetailDto>.Success(result);


        }


















       public ServiceResponseDto<ProductSaleOptionColorDetailDto> DeleteProductSaleOptionColor(
           int productSaleOptionColorId)
        {
            var productcolor = _shopDbContext.ProductSaleOptionColors.FirstOrDefault(
                x => x.Id == productSaleOptionColorId);

            if (productcolor == null)
            {
                return ServiceResponseDto<ProductSaleOptionColorDetailDto>.Fail("محصول موجود نمیباشد");
            }


            _shopDbContext.ProductSaleOptionColors.Remove(productcolor);

            _shopDbContext.SaveChanges();

            return ServiceResponseDto<ProductSaleOptionColorDetailDto>.Success();

        }











         public ServiceResponseDto<ProductSaleOptionColorDetailDto> UpdateProductSaleOptionColor(
             ProductSaleOptionColorUpdateDto model)
        {

            if (model == null)
            {
                return ServiceResponseDto<ProductSaleOptionColorDetailDto>.Fail("اطلاعات ساخت موحود نمیباشد");
            }


            var ProductColor = _shopDbContext.ProductSaleOptionColors.FirstOrDefault(x => x.Id == model.Id);

            if (ProductColor == null)
            {
                return ServiceResponseDto<ProductSaleOptionColorDetailDto>.Fail("رنگ موردنظر یافت نشد");
            }

            ProductColor.Color = model.Color;
            ProductColor.Price = model.Price;
            ProductColor.ImageUrl = model.ImageUrl;
            ProductColor.HexCode = model.HexCode;


            _shopDbContext.SaveChanges();



            var result = new ProductSaleOptionColorDetailDto();

            result.Id = ProductColor.Id;
            result.ProductSaleOptionId = ProductColor.ProductSaleOptionId;
            result.Color = ProductColor.Color;
            result.HexCode = ProductColor.HexCode;
            result.Price = ProductColor.Price;
            result.ImageUrl = ProductColor.ImageUrl;

            return ServiceResponseDto<ProductSaleOptionColorDetailDto>.Success(result);






        }
    }
}
