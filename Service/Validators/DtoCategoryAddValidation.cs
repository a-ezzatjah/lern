using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Entities;
using FluentValidation;
using Microsoft.EntityFrameworkCore;
using ServiceContract.DTO.DtoCategory;

namespace Service.Validators
{
    public class DtoCategoryAddValidation : AbstractValidator<AddDtoCategory>
    {

        private readonly ShopDbContext _shopDbContext;
        public DtoCategoryAddValidation(ShopDbContext shopDbContext)
        {
            _shopDbContext = shopDbContext;


            RuleFor(x => x.Name)
                .NotEmpty().WithMessage("دسته بندی خالی میباشد")
                .MustAsync(async (x, CancellationToken) => { return !await _shopDbContext.categories.AnyAsync(s => s.Name == x); })
                .MaximumLength(50).WithMessage("تعداد کاراکتر های نام دسته بندی بیش از حد مجاز میباشد")
                .Must(x => !string.IsNullOrWhiteSpace(x))
                .WithMessage("نام دسته بندی نمی‌تواند فقط فاصله باشد");


            RuleFor(x => x.Slug)
                .Must(x => !string.IsNullOrWhiteSpace(x)).WithMessage("اسلاگ نمیتواند خالی باشد")
                .MaximumLength(50).WithMessage("اسلاگ نباید بیشتر از 50 کاراکتر باشد")
                .MustAsync(async (slug, CancellationToken) => { return !await _shopDbContext.categories.AnyAsync(s => s.Slug == slug); })
                .WithMessage("اسلاگ تکراری میباشد");



            RuleFor(x => x.ParentId)
                .MustAsync(async (x, CancellationToken) =>
                { if (x == null) return true; return await _shopDbContext.categories.AnyAsync(s => s.Id == x.Value); })
                .WithMessage("آیدی برای دسته پدر وجود ندارد")
            .GreaterThanOrEqualTo(1)
            .When(x => x.ParentId.HasValue);


            RuleFor(x => x.SortOrder)
           .GreaterThanOrEqualTo(0)
           .WithMessage("ترتیب نمایش نمی‌تواند منفی باشد");

        }




    }
}
