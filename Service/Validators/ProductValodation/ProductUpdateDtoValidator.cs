using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Entities;
using FluentValidation;
using Microsoft.EntityFrameworkCore;
using ServiceContract.DTO.DtoProduct;
namespace Service.Validators.ProductValodation
{
   public class ProductUpdateDtoValidator : AbstractValidator<ProductUpdateDto>
    {

        private readonly ShopDbContext _shopDbContext;


        public ProductUpdateDtoValidator(ShopDbContext shopDbContext)
        {

            _shopDbContext = shopDbContext;

            RuleFor(x => x.Name)
                .Must(x => !string.IsNullOrWhiteSpace(x)).WithMessage("نام محصول معتبر نمیباشد")
                .MustAsync(async (model, name, CancellationToken) =>
                { return !await _shopDbContext.Products.AnyAsync(x => x.Name == name && x.Id != model.Id); });


            RuleFor(x => x.Slug)
               .Must(x => !string.IsNullOrWhiteSpace(x)).WithMessage("اسلاگ محصول معتبر نمیباشد")
               .MustAsync(async (model, slug, CancellationToken) =>
               { return !await _shopDbContext.Products.AnyAsync(x => x.Slug == slug && x.Id != model.Id); });



            RuleFor(x => x.Id)
                .NotNull().WithMessage("شناسه الزامی میباشد")
                .GreaterThanOrEqualTo (1).WithMessage("شناسه محصول نا معتبر هست ");




            RuleFor(x => x.DiscountValue)
                .GreaterThanOrEqualTo(0).WithMessage("مقدار تخفیف نمی‌تواند منفی باشد")
                .When(x => x.DiscountValue.HasValue);

            RuleFor(x => x.DiscountType)
                .NotNull().WithMessage("نوع تخفیف باید مشخص شود")
                .When(x => x.DiscountValue.HasValue && x.DiscountValue.Value > 0);

            RuleFor(x => x.CategoryIds)
                .NotEmpty().WithMessage("حداقل یک دسته‌بندی باید انتخاب شود");

            RuleForEach(x => x.CategoryIds)
                .GreaterThan(0).WithMessage("شناسه دسته‌بندی معتبر نیست");



            RuleFor(x => x.CategoryIds)
                .MustAsync(async (categoryIds, cancellationToken) =>
                {
                    var distinctIds = categoryIds.Distinct().ToList();
                    var count = await _shopDbContext.Categories.CountAsync(c => distinctIds.Contains(c.Id), cancellationToken);
                    return count == distinctIds.Count;
                });

            RuleForEach(x => x.SaleOptions).ChildRules(option =>
            {
                option.RuleFor(x => x.Title)
                    .Must(x => !string.IsNullOrWhiteSpace(x)).WithMessage("عنوان حالت فروش الزامی میباشد");

                option.RuleFor(x => x.BasePrice)
                    .GreaterThan(0).WithMessage("قیمت پایه حالت فروش باید بیشتر از صفر باشد");

                option.RuleFor(x => x.Step)
                    .GreaterThan(0).WithMessage("گام تعداد باید بیشتر از صفر باشد");

                option.RuleForEach(x => x.SaleOptionColors).ChildRules(color =>
                {
                    color.RuleFor(x => x.Color)
                        .Must(x => !string.IsNullOrWhiteSpace(x)).WithMessage("نام رنگ الزامی میباشد");

                    color.RuleFor(x => x.Price)
                        .GreaterThanOrEqualTo(0).WithMessage("قیمت رنگ نمی‌تواند منفی باشد");
                });
            });










        }






    }
}
