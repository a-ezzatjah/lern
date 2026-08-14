using FluentValidation;
using ServiceContract.DTO.DtoProductSaleOptionColor;

namespace Service.Validators.ProductSaleOptionColorValidation
{
    public class ProductSaleOptionColorUpdateDtoValidator : AbstractValidator<ProductSaleOptionColorUpdateDto>
    {
        public ProductSaleOptionColorUpdateDtoValidator()
        {
            RuleFor(x => x.Id)
                .GreaterThan(0).WithMessage("شناسه رنگ معتبر نیست");

            RuleFor(x => x.Color)
                .NotEmpty().WithMessage("نام رنگ الزامی است")
                .MaximumLength(50).WithMessage("نام رنگ بیش از حد مجاز است");

            RuleFor(x => x.HexCode)
                .MaximumLength(20).WithMessage("کد رنگ بیش از حد مجاز است");
        }
    }
}
