using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FluentValidation;
using ServiceContract.DTO.DtoProduct;
namespace Service.Validators
{
   public  class DtoProductUodateValidation : AbstractValidator<DtoProductUpdate>
    {

        public DtoProductUodateValidation()
        {

            RuleFor(x => x.Name)
                .Must(x=>!string.IsNullOrWhiteSpace(x)).WithMessage("نام محصول معتبر نمیباشد");

            RuleFor(x => x.Id)
                .NotNull().WithMessage("شناسه الزامی میباشد")
                .GreaterThanOrEqualTo (1).WithMessage("شناسه محصول نا معتبر هست ");


        }






    }
}
