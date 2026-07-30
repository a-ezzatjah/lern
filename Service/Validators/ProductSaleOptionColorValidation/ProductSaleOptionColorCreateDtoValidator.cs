using FluentValidation;
using ServiceContract.DTO.DtoProductSaleOptionColor;

namespace Service.Validators.ProductSaleOptionColorValidation
{
    public class ProductSaleOptionColorCreateDtoValidator : AbstractValidator<ProductSaleOptionColorCreateDto>
    {
        public ProductSaleOptionColorCreateDtoValidator()
        {
            RuleFor(x => x.ProductSaleOptionId)
                .GreaterThan(0).WithMessage("شناسه گزینه فروش معتبر نیست");

            RuleFor(x => x.Color)
                .NotEmpty().WithMessage("نام رنگ الزامی است")
                .MaximumLength(50).WithMessage("نام رنگ بیش از حد مجاز است");

            RuleFor(x => x.Price)
                .GreaterThanOrEqualTo(0)
                .When(x => x.Price.HasValue)
                .WithMessage("قیمت نمی‌تواند منفی باشد");

            RuleFor(x => x.HexCode)
                .MaximumLength(20).WithMessage("کد رنگ بیش از حد مجاز است");

            RuleFor(x => x.ImageUrl)
                .MaximumLength(500).WithMessage("آدرس تصویر بیش از حد مجاز است");
        }
    }
}
