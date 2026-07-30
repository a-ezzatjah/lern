using FluentValidation;
using ServiceContract.DTO.DtoProductImage;

namespace Service.Validators.ProductImageValidation
{
    public class ProductImageCreateDtoValidator : AbstractValidator<ProductImageCreateDto>
    {
        public ProductImageCreateDtoValidator()
        {
            RuleFor(x => x.ProductId)
                .GreaterThan(0)
                .When(x => x.ProductId.HasValue)
                .WithMessage("شناسه محصول معتبر نیست");

            RuleFor(x => x.VariantId)
                .GreaterThan(0)
                .When(x => x.VariantId.HasValue)
                .WithMessage("شناسه تنوع محصول معتبر نیست");

            RuleFor(x => x.ImageUrl)
                .NotEmpty().WithMessage("آدرس تصویر الزامی است")
                .MaximumLength(500).WithMessage("آدرس تصویر بیش از حد مجاز است");

            RuleFor(x => x.AltText)
                .MaximumLength(200).WithMessage("متن جایگزین تصویر بیش از حد مجاز است");

            RuleFor(x => x.SortOrder)
                .GreaterThanOrEqualTo(0).WithMessage("ترتیب نمایش نمی‌تواند منفی باشد");
        }
    }
}
