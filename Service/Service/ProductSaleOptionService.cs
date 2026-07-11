using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AutoMapper;
using Entities;
using ServiceContract.DTO.DtoCommit;
using ServiceContract.DTO.DtoProductSaleOption;
using ServiceContract.Interfaces;

namespace Service.Service
{
    public class ProductSaleOptionService : IProductSaleOptionService
    {



      private readonly IMapper _mapper;
      private readonly ShopDbContext _context;
      
        public ProductSaleOptionService(ShopDbContext context, IMapper mapper)
            {

            _context = context;
            _mapper = mapper;

            }












        public DtoResponse<DtoProductAdminSaleOption> AddProductSaleOptionAsync(DtoAddProductSaleOption model)
        {
            var productsaleoption = _mapper.Map<ProductSaleOption>(model);
            _context.ProductSaleOptions.Add(productsaleoption);
            _context.SaveChanges();
            
            var result = _mapper.Map<DtoProductAdminSaleOption>(productsaleoption);

            return DtoResponse<DtoProductAdminSaleOption>.Success(result);

        }

        public DtoResponse<DtoProductAdminSaleOption> AddProductSaleOptionAsync(DtoAddProductSaleOption model)
        {
            throw new NotImplementedException();
        }


        public DtoResponse<DtoProductAdminSaleOption> DeleteProductSaleOptionAsync(int id)
        {
            throw new NotImplementedException();
        }


        public DtoResponse<DtoProductAdminSaleOption> GetProductSaleOptionByIdAsync(int id)
        {
            throw new NotImplementedException();
        }
        public DtoResponse<DtoProductAdminSaleOption> UpdateProductSaleOptionAsync(UpdateDtoProductSaleOption model)
        {
            throw new NotImplementedException();
        }
    }

    








    
}
