using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FluentValidation;
using FluentValidation.Validators;
using ServiceContract.DTO.DtoProductSaleOption;

namespace Service.Validators.SaleOptionValidation
{
   public class ProductSaleOptionCreateDtoValidator : AbstractValidator<ProductSaleOptionCreateDto>
    {

        public ProductSaleOptionCreateDtoValidator()
        {

            RuleFor(x => x.Title).NotEmpty().WithMessage("حالت فروش وارد نشده است");


        }
            

        








    }
}
