using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DTO;
using FluentValidation;
using ServiceContract.DTO.DtoProduct;

namespace Service.Validators
{
   public class DtoProductAddValidation :  AbstractValidator<DtoproductAdd>
    {

        public DtoProductAddValidation()
        {

            RuleFor(x => x.Name)
                .Must(x=>!string.IsNullOrWhiteSpace(x)).WithMessage("نام محصول الزامی میباشد")
                .MaximumLength(100).WithMessage("تعداد کاراکتر نام بیشتر از حد مجاز میباشد");

            
            RuleFor(x => x.Price)
                .NotNull().WithMessage("قیمت الزامی میباشد")
                .GreaterThan(0).WithMessage("قیمت نمیتواند کوچک تر از 0 باشه ");


        }





    }
}
