using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using DTO;
using Entities;
using FluentValidation;
using Microsoft.EntityFrameworkCore;
using ServiceContract.DTO.DtoProduct;

namespace Service.Validators
{
    public class DtoProductAddValidation : AbstractValidator<DtoproductAdd>
    {
        private readonly ShopDbContext _shopDbContext;

        public DtoProductAddValidation(ShopDbContext shopDbContext)
        {

            _shopDbContext = shopDbContext;



            RuleFor(x => x.Name)
                .Must(x => !string.IsNullOrWhiteSpace(x)).WithMessage("نام محصول الزامی میباشد")
                .MaximumLength(100).WithMessage("تعداد کاراکتر نام بیشتر از حد مجاز میباشد");

            RuleFor(x => x.Slug)
                .Must(x => !string.IsNullOrWhiteSpace(x)).WithMessage("اسلاگ الزامی میباشد")
                .MustAsync(async (slug, cancellationToken) =>
                    !await _shopDbContext.Products.AnyAsync(x => x.Slug == slug, cancellationToken))
                .MaximumLength(50).WithMessage("مقادیر حروف بیش از حد مجاز میباشد");


            RuleFor(x => x.DiscountValue).GreaterThan(0).WithMessage("تخفیف باید بیشتر از 0 باشد")
                .When(x => x.DiscountValue.HasValue);


            RuleFor(x => x.CategoryIds)
                 .NotEmpty().WithMessage("باید حتما یک دسته بندی انتخاب شود")
                 .NotNull().WithMessage("باید حتما یک دسته بندی انتخاب شود");


            RuleForEach(x => x.CategoryIds).GreaterThan(0).WithMessage("شناسه دسته‌بندی معتبر نیست");

            RuleFor(x => x.CategoryIds)
           .MustAsync(async (categoryIds, cancellationToken) =>
           {
               var distinctIds = categoryIds.Distinct().ToList();
               var count = await _shopDbContext.Categories.CountAsync(c => distinctIds.Contains(c.Id), cancellationToken);
               return count == distinctIds.Count;
           }).When(x => x.CategoryIds != null && x.CategoryIds.Any())
             .WithMessage("یک یا چند دسته‌بندی معتبر نیستند");

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




            //          private async Task<bool> BeUniqueName(string? name, CancellationToken cancellationToken)
            //    {
            //        if (string.IsNullOrWhiteSpace(name))
            //            return true;

            //        return !await _context.Products
            //            .AnyAsync(x => x.Name == name, cancellationToken);
            //    }

            //    private async Task<bool> BeUniqueSlug(string? slug, CancellationToken cancellationToken)
            //    {
            //        if (string.IsNullOrWhiteSpace(slug))
            //            return true;

            //        return !await _context.Products
            //            .AnyAsync(x => x.Slug == slug, cancellationToken);
            //    }

            //    private async Task<bool> AllCategoriesExist(List<int> categoryIds, CancellationToken cancellationToken)
            //    {
            //        if (categoryIds == null || !categoryIds.Any())
            //            return false;

            //        var distinctIds = categoryIds.Distinct().ToList();

            //        var count = await _context.Categories
            //            .CountAsync(x => distinctIds.Contains(x.Id), cancellationToken);

            //        return count == distinctIds.Count;
            //    }
            //}
        }









    }






}
