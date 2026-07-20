using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AutoMapper;
using Entities;
using Microsoft.EntityFrameworkCore;
using ServiceContract.DTO.DtoCommit;
using ServiceContract.DTO.DtoSaleOptionColor;
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















      public  ServiceResponseDto<SaleOptionColorDetailDto>CreateProdactColor(SaleOptionColorCreateDto model)
        {

            if (model == null)
            {
               return ServiceResponseDto<SaleOptionColorDetailDto>.Fail("اطلاعات ساخت موحود نمیباشد");
            }

            var ProductColor = new SaleOptionColor();

            ProductColor.SaleOptionId = model.SaleOptionId;
            ProductColor.Color = model.Color;
            ProductColor.HexCode = model.HexCode;
            ProductColor.Price = model.Price;
            ProductColor.ImageUrl = model.ImageUrl;


            _shopDbContext.SaleOptionColors.Add(ProductColor);
            _shopDbContext.SaveChanges();



            var result = new SaleOptionColorDetailDto();

            
            result.Color = ProductColor.Color;
            result.HexCode = ProductColor.HexCode;
            result.Price = ProductColor.Price;
            result.ImageUrl = ProductColor.ImageUrl;


            return ServiceResponseDto<SaleOptionColorDetailDto>.Success(result);


        }



















       public ServiceResponseDto<SaleOptionColorDetailDto>DeleteProdactColor(int ProductColorid)
        {
            var productcolor = _shopDbContext.SaleOptionColors.FirstOrDefault(x => x.Id == ProductColorid);

            if (productcolor == null)
            {
                return ServiceResponseDto<SaleOptionColorDetailDto>.Fail("محصول موجود نمیباشد");
            }


            _shopDbContext.SaleOptionColors.Remove(productcolor);

            _shopDbContext.SaveChanges();

            return ServiceResponseDto<SaleOptionColorDetailDto>.Success();

        }











         public ServiceResponseDto<SaleOptionColorDetailDto> UpdateProductColor(SaleOptionColorUpdateDto model)
        {

            if (model == null)
            {
                return ServiceResponseDto<SaleOptionColorDetailDto>.Fail("اطلاعات ساخت موحود نمیباشد");
            }


            var ProductColor = _shopDbContext.SaleOptionColors.FirstOrDefault(x => x.Id == model.Id);

            if (ProductColor == null)
            {
                return ServiceResponseDto<SaleOptionColorDetailDto>.Fail("رنگ موردنظر یافت نشد");
            }

            ProductColor.Color = model.Color;
            ProductColor.Price = model.Price;
            ProductColor.ImageUrl = model.ImageUrl;
            ProductColor.HexCode = model.HexCode;


            _shopDbContext.SaveChanges();



            var result = new SaleOptionColorDetailDto();

            result.Id = ProductColor.Id;
            result.SaleOptionId = ProductColor.SaleOptionId;
            result.Color = ProductColor.Color;
            result.HexCode = ProductColor.HexCode;
            result.Price = ProductColor.Price;
            result.ImageUrl = ProductColor.ImageUrl;

            return ServiceResponseDto<SaleOptionColorDetailDto>.Success(result);






        }
    }
}
