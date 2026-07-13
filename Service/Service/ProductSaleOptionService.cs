using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AutoMapper;
using Entities;
using Microsoft.EntityFrameworkCore;
using ServiceContract.DTO.DtoCommit;
using ServiceContract.DTO.DtoProductSaleOption;
using ServiceContract.DTO.DtoSaleOptionColor;
using ServiceContract.Interfaces;

namespace Service.Service
{
    public class ProductSaleOptionService : IProductSaleOptionService
    {



      private readonly IMapper _mapper;
      private readonly ShopDbContext _shopdbcontext;
      
        public ProductSaleOptionService(ShopDbContext context, IMapper mapper)
            {

            _shopdbcontext = context;
            _mapper = mapper;

            }







        public async Task<DtoResponse<DtoProductAdminSaleOption>> AddProductSaleOptionAsync(DtoAddProductSaleOption model)
        {

            if (model == null)
            {
                DtoResponse<DtoProductAdminSaleOption>.Fail("حالت فروش  وارد نشده است");
            }

            var productsaleoption = _mapper.Map<ProductSaleOption>(model);
         

            _shopdbcontext.Add(productsaleoption);
           await _shopdbcontext.SaveChangesAsync();

            var result = _mapper.Map<DtoProductAdminSaleOption>(productsaleoption);
            return DtoResponse<DtoProductAdminSaleOption>.Success(result);

        }

      


        public async Task<DtoResponse<bool>> DeleteProductSaleOptionAsync(int id)
        {
            var productsaleoption = await _shopdbcontext.ProductSaleOptions.FirstOrDefaultAsync(x => x.Id == id);

            if(productsaleoption == null)
            {
                return DtoResponse<bool>.Fail("گزینه فروش موجود نمیباشد");
            }

            _shopdbcontext.ProductSaleOptions.Remove(productsaleoption);
            await _shopdbcontext.SaveChangesAsync();
           return DtoResponse<bool>.Success(true);

        }


        //public DtoResponse<DtoProductAdminSaleOption> GetProductSaleOptionByIdAsync(int id)
        //{
        //    // فعلا نمیخواد چون توی productservise میگیریم 

        //}



        public async Task<DtoResponse<DtoProductAdminSaleOption>> UpdateProductSaleOptionAsync(UpdateDtoProductSaleOption model)
        {
            if (model == null)
            {
                DtoResponse<DtoProductAdminSaleOption>.Fail("آپیدیت وارد نشده است");
            }

            if (model.Id == null)
            {
                DtoResponse<DtoProductAdminSaleOption>.Fail("آپیدیت وارد نشده است");
            }

            var productsaleoption =await _shopdbcontext.ProductSaleOptions.Include(x => x.SaleOptionColors).FirstOrDefaultAsync(x => x.Id == model.Id);

           


            _mapper.Map(model, productsaleoption);

            _shopdbcontext.SaleOptionColors.RemoveRange(productsaleoption.SaleOptionColors);

            productsaleoption.SaleOptionColors = model.SaleOptionColors.Select(x => new SaleOptionColor
            {
                Color = x.Color,
                HexCode = x.HexCode,
                ImageUrl = x.ImageUrl,
                Price = x.Price
            }).ToList();

            await _shopdbcontext.SaveChangesAsync();

            var result = _mapper.Map<DtoProductAdminSaleOption>(productsaleoption);

           return  DtoResponse<DtoProductAdminSaleOption>.Success(result);






        }




    }

    








    
}
