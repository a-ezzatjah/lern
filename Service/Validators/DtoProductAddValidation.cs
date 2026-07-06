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
   public class DtoProductAddValidation :  AbstractValidator<DtoproductAdd>
    {
        private readonly ShopDbContext _shopDbContext;

        public DtoProductAddValidation(ShopDbContext shopDbContext)
        {

            _shopDbContext = shopDbContext;



            RuleFor(x => x.Name)
                .Must(x=>!string.IsNullOrWhiteSpace(x)).WithMessage("نام محصول الزامی میباشد")
                .MaximumLength(100).WithMessage("تعداد کاراکتر نام بیشتر از حد مجاز میباشد");

            RuleFor(x => x.Slug)
                .Must(x => !string.IsNullOrWhiteSpace(x)).WithMessage("اسلاگ الزامی میباشد")
                .MustAsync(async (slug, CancellationToken) => {!await return _shopDbContext.Categories.AllAsync(x => x.Slug == slug, cancellationToken) })
                .MaximumLength(50).WithMessage("مقادیر حروف بیش از حد مجاز میباشد");


            RuleFor(x => x.DiscountValue).GreaterThan(0).WithMessage("تخفیف باید بیشتر از 0 باشد")
                .When(x => x.DiscountValue.HasValue);


            RuleFor(x => x.CategoryIds).NotEmpty().WithMessage("باید حتما یک دسته بندی انتخاب شود");

            RuleForEach(x => x.CategoryIds).GreaterThan(0).WithMessage("شناسه دسته‌بندی معتبر نیست");

               RuleFor(x => x.CategoryIds)
              .MustAsync(async (categoryIds, cancellationToken) =>
               await _shopDbContext.Categories.AnyAsync(c => categoryIds.Contains(c.Id), cancellationToken)).WithMessage("یک یا چند دسته‌بندی معتبر نیستند");














        }





    }
}
