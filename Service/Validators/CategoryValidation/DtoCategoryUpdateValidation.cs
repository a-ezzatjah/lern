using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Entities;
using FluentValidation;
using Microsoft.EntityFrameworkCore;
using ServiceContract.DTO.DtoCategory;

namespace Service.Validators.CategoryValidation
{
    public class DtoCategoryUpdateValidation : AbstractValidator<DtoCategoryUpdate>
    {

      private readonly ShopDbContext _shopDbContext;


        public DtoCategoryUpdateValidation(ShopDbContext shopDbContext)
        {

            _shopDbContext = shopDbContext;



            RuleFor(x => x.Id)
                .NotEmpty().WithMessage("مقدار آیدی وارد نشده است ")
                .NotNull().WithMessage("مقدار آیدی معتبر نمیباشد")
                .GreaterThanOrEqualTo(1).WithMessage("مقدارآیدی نا معتبر میباشد");

            RuleFor(x => x.Name)
                .NotNull().WithMessage("نام دسته بندی وارد نشده است")
                .NotEmpty().WithMessage("نام دسته بندی وارد نشده است")
                .MustAsync(async (model,name, CancellationToken) => 
                { return !await _shopDbContext.Categories.AnyAsync(s => s.Name == name && s.Id != model.Id); })
                .WithMessage("نام دسته بندی تکراری میباشد")
                .MaximumLength(50).WithMessage("تعداد کاراکتر های نام دسته بیش از حد مجاز میباشد");



            RuleFor(x => x.Slug)
              .Must(x => !string.IsNullOrWhiteSpace(x)).WithMessage("اسلاگ نمیتواند خالی باشد")
              .MaximumLength(50).WithMessage("اسلاگ نباید بیشتر از 50 کاراکتر باشد")
              .MustAsync(async (model,slug, CancellationToken) =>
              { return !await _shopDbContext.Categories.AnyAsync(s => s.Slug == slug && s.Id != model.Id); })
              .WithMessage("اسلاگ تکراری میباشد");




            RuleFor(x => x.ParentId)
                .MustAsync(async (x, CancellationToken) => { return await _shopDbContext.Categories.AnyAsync(s => s.Id == x); })
                .When(x => x.ParentId.HasValue)
                .NotEqual(x => x.Id)
                .GreaterThanOrEqualTo(1).When(x => x.ParentId.HasValue);




            RuleFor(x => x.SortOrder)
           .GreaterThanOrEqualTo(0)
           .WithMessage("ترتیب نمایش نمی‌تواند منفی باشد");



        }





    }








}
