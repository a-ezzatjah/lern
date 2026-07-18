using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using FluentValidation;
using ServiceContract.DTO.DtoProductSaleOption;

namespace Service.Validators.SaleOptionValidation
{
    public class ProductSaleOptionUpdateDtoValidator : AbstractValidator<ProductSaleOptionUpdateDto>
    {
        public ProductSaleOptionUpdateDtoValidator()
        {
            RuleFor(x => x.Id)
                .GreaterThanOrEqualTo(0).WithMessage("شناسه گزینه فروش معتبر نیست");

            RuleFor(x => x.Title)
                .NotEmpty().WithMessage("عنوان حالت فروش الزامی می‌باشد");

            RuleFor(x => x.BasePrice)
                .GreaterThan(0).When(x => x.BasePrice.HasValue)
                .WithMessage("قیمت پایه باید بیشتر از صفر باشد");

            RuleFor(x => x.Step)
                .GreaterThan(0).WithMessage("گام تعداد باید بیشتر از صفر باشد");
        }
    }
}
