using Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Service.Mapping;
using ServiceContract.Interfaces;
using FluentValidation;
using DTO;
using Service.Service;
using Service.Validators.ProductValodation;
using lern.Infrastructure;
using Microsoft.AspNetCore.Authentication.Cookies;


var builder = WebApplication.CreateBuilder(args);


builder.Services.AddMemoryCache();
builder.Services.AddScoped<IProductService, ProductService>();
builder.Services.AddScoped<ICategoryService, CategoryService>();
builder.Services.AddScoped<IProductSaleOptionService, ProductSaleOptionService>();
builder.Services.AddScoped<IProductSaleOptionColorService, ProductSaleOptionColorService>();
builder.Services.AddScoped<IProductVariantService, ProductVariantService>();
builder.Services.AddScoped<IPricingService, PricingService>();
builder.Services.AddScoped<IUserAuthService, UserAuthService>();
builder.Services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
    .AddCookie(options =>
    {
        options.LoginPath = "/login";
        options.Cookie.Name = "lern.auth";
        options.Cookie.HttpOnly = true;
        options.Cookie.SameSite = SameSiteMode.Lax;
        options.SlidingExpiration = true;
        options.Events.OnRedirectToLogin = context =>
        {
            if (context.Request.Path.StartsWithSegments("/api")) { context.Response.StatusCode = StatusCodes.Status401Unauthorized; return Task.CompletedTask; }
            context.Response.Redirect(context.RedirectUri);
            return Task.CompletedTask;
        };
    });
builder.Services.AddAuthorization();
builder.Services.AddControllersWithViews();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddAutoMapper(cfg =>
{
    cfg.AddProfile<ProductMappingProfile>();
    cfg.AddProfile<CategoryMappingProfile>();
    cfg.AddProfile<ProductSaleOptionColorMappingProfile>();
    cfg.AddProfile<ProductImageMappingProfile>();
    cfg.AddProfile<ProductVariantMappingProfile>();
});
builder.Services.AddDbContext<ShopDbContext>(option => option.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnectionstring")));
builder.Services.AddValidatorsFromAssemblyContaining<ProductCreateDtoValidator>();

var app = builder.Build();

// اطمینان از ایجاد جدول آدرس هنگام اجرای برنامه (در محیط توسعه/استقرار اولیه).
// در صورت در دسترس نبودن دیتابیس، اجرای برنامه متوقف نمی‌شود و خطا در لاگ ثبت می‌گردد.
try
{
    await using var scope = app.Services.CreateAsyncScope();
    var db = scope.ServiceProvider.GetRequiredService<ShopDbContext>();
    await db.Database.MigrateAsync();
    await CategoryCatalogSeeder.SeedAsync(db);
}
catch (Exception ex)
{
    app.Logger.LogWarning(ex, "اجرای migration های دیتابیس انجام نشد.");
}

app.UseSwagger();
app.UseSwaggerUI();
app.UseStaticFiles();
app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();
app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.Run();

