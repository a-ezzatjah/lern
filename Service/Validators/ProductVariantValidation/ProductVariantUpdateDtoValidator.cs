using FluentValidation;
using ServiceContract.DTO.DtoProductVariant;

namespace Service.Validators.ProductVariantValidation
{
    public class ProductVariantUpdateDtoValidator : AbstractValidator<ProductVariantUpdateDto>
    {
        public ProductVariantUpdateDtoValidator()
        {
            RuleFor(x => x.Id)
                .GreaterThan(0).WithMessage("شناسه تنوع محصول معتبر نیست");

            RuleFor(x => x.ProductSaleOptionId)
                .GreaterThan(0).WithMessage("شناسه گزینه فروش معتبر نیست");

            RuleFor(x => x.ProductSaleOptionColorId)
                .GreaterThan(0)
                .When(x => x.ProductSaleOptionColorId.HasValue)
                .WithMessage("شناسه رنگ معتبر نیست");

            RuleFor(x => x.Sku)
                .NotEmpty().WithMessage("شناسه انبارداری الزامی است")
                .MaximumLength(100).WithMessage("شناسه انبارداری بیش از حد مجاز است");

            RuleFor(x => x.Price)
                .GreaterThanOrEqualTo(0).WithMessage("قیمت نمی‌تواند منفی باشد");

            RuleFor(x => x.DiscountValue)
                .GreaterThanOrEqualTo(0)
                .When(x => x.DiscountValue.HasValue)
                .WithMessage("مقدار تخفیف نمی‌تواند منفی باشد");

            RuleFor(x => x.StockQuantity)
                .GreaterThanOrEqualTo(0).WithMessage("موجودی نمی‌تواند منفی باشد");

            RuleFor(x => x.ReservedQuantity)
                .GreaterThanOrEqualTo(0).WithMessage("موجودی رزروشده نمی‌تواند منفی باشد")
                .LessThanOrEqualTo(x => x.StockQuantity)
                .WithMessage("موجودی رزروشده نمی‌تواند بیشتر از موجودی کل باشد");
        }
    }
}
