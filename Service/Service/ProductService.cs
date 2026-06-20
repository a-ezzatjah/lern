
using System.ComponentModel;
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
using ServiceContract.Enums;
using ServiceContract.Interfaces;
using ServiceContract.Quaries;

namespace Service.Service
{
    public class ProductService : IProductService
    {
        private readonly IMapper _mapper;
        private readonly ShopDbContext _shopDbContext;
        private readonly IValidator<DtoproductAdd> _validations;
        private readonly IValidator<DtoProductUpdate> _updateValidator;




        public ProductService(ShopDbContext shopDbContext, IMapper mapper, IValidator<DtoproductAdd> validationRules, IValidator<DtoProductUpdate> Update)
        {
            _mapper = mapper;
            _shopDbContext = shopDbContext;
            _validations = validationRules;
            _updateValidator = Update;
        }




        public DtoResponse<DtoProduct> AddProduct(DtoproductAdd model)
        {

            if (model == null)
                return DtoResponse<DtoProduct>.Fail("داده نامعتبر است");

            var validationResult = _validations.Validate(model);
            if (!validationResult.IsValid)
                return DtoResponse<DtoProduct>.Fail("اطلاعات محصول معتبر نیست");



            var oldproduct = _shopDbContext.products.Any(x => x.Name.ToLower() == model.Name.ToLower());
            if (oldproduct)
            {
                return DtoResponse<DtoProduct>.Fail("محصول تکراری میباشد");
            }


            var product = _mapper.Map<Product>(model);

            _shopDbContext.products.Add(product);

            _shopDbContext.SaveChanges();


            var result = _mapper.Map<DtoProduct>(product);

            return DtoResponse<DtoProduct>.Success(result);


        }








        public DtoResponse<bool> Delete(int productid)
        {
            var poroduct = GetEntityById(productid);
            if (poroduct == null)
            {
                return DtoResponse<bool>.Fail("محصول مورد نظر یافت نشد");
            }



            _shopDbContext.products.Remove(poroduct);


            _shopDbContext.SaveChanges();

            return DtoResponse<bool>.Success();
        }






        public string GetBranchName(int? branchid)
        {
            throw new NotImplementedException();
        }










        public DtoProduct? GetById(int productId)
        {
         
            return _shopDbContext.products
       .Where(x => x.Id == productId)
       .ProjectTo<DtoProduct>(_mapper.ConfigurationProvider)
       .FirstOrDefault();


        }






        public Product? GetEntityById(int product)
        {
            return _shopDbContext.products.FirstOrDefault(x => x.Id == product);

        }










        public async Task<PageResult<DtoProduct>> GetFilterAsync(ProductQuery query)
        {
            IQueryable<Product> Product = _shopDbContext.products.AsNoTracking();

            //var Product = _shopDbContext.products;

            if (!string.IsNullOrWhiteSpace(query.SearchText))
            {
                if (query.SearchType == EnumProductSearchType.All)
                {
                    Product = Product.Where(x =>
                        x.Name.Contains(query.SearchText) ||
                        x.Description != null && x.Description.Contains(query.SearchText));
                }
                else if (query.SearchType == EnumProductSearchType.Name)
                {
                    Product = Product.Where(x => x.Name.Contains(query.SearchText));
                }
                else if (query.SearchType == EnumProductSearchType.Description)
                {
                    Product = Product.Where(x =>
                            x.Description != null &&
                            x.Description.Contains(query.SearchText));
                }
                else if (
                    query.SearchType == EnumProductSearchType.Price
                    && decimal.TryParse(query.SearchText, out var priceValue))
                {
                    Product = Product.Where(x => x.Price == priceValue);
                }
            }

            Product = (query.SortType, query.Order) switch
            {
                (EnumProductSortType.Name, OrderEnum.ASC) => Product.OrderBy(x => x.Name),
                (EnumProductSortType.Name, OrderEnum.DESC) => Product.OrderByDescending(x => x.Name),



                (EnumProductSortType.Price, OrderEnum.ASC) => Product.OrderBy(x => x.Price),
                (EnumProductSortType.Price, OrderEnum.DESC) => Product.OrderByDescending(x => x.Price),

                _ => Product.OrderBy(x => x.Id)
            };


            var totalCount = await Product.CountAsync();


            Product = Product
           .Skip((query.Page - 1) * query.PageSize)
           .Take(query.PageSize);





            var items = await Product
           .ProjectTo<DtoProduct>(_mapper.ConfigurationProvider)
           .ToListAsync();



            return new PageResult<DtoProduct>
            {
                Items = items,
                TotalCount = totalCount,
                Page = query.Page,
                PageSize = query.PageSize
            };

        }





        public List<DtoSearchOption> GetSelect()
        {
            var result = new List<DtoSearchOption>();

            result.Add(new DtoSearchOption { Key = nameof(DtoProduct.Name), Title = "نام" });
            result.Add(new DtoSearchOption { Key = nameof(DtoProduct.Description), Title = "توضیحات" });
            result.Add(new DtoSearchOption { Key = nameof(DtoProduct.Price), Title = "قیمت" });

            return result;
        }





        public DtoResponse<DtoProduct> Update(DtoProductUpdate model)
        {

            if (model == null)
                return DtoResponse<DtoProduct>.Fail("داده نامعتبر است");


            var ValidationResult = _updateValidator.Validate(model);

            if (!ValidationResult.IsValid)
            {
                return DtoResponse<DtoProduct>.Fail("اطلاعات وارد شده معتبر نیست");

            }


            var product = GetEntityById(model.Id.Value);
            if (product == null)
            {
                return DtoResponse<DtoProduct>.Fail("محصول موجود نمیباشد ");
            }

 


            _mapper.Map(model, product);

            _shopDbContext.SaveChanges();



             var result = _mapper.Map<DtoProduct>(product);

            return DtoResponse<DtoProduct>.Success(result);
        }
    }
}
