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
using Microsoft.AspNetCore.RateLimiting;
using System.Threading.RateLimiting;


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
        options.AccessDeniedPath = "/account/access-denied";
        options.Cookie.Name = "lern.auth";
        options.Cookie.HttpOnly = true;
        options.Cookie.Path = "/";
        options.Cookie.SameSite = SameSiteMode.Lax;
        options.Cookie.IsEssential = true;
        options.ExpireTimeSpan = TimeSpan.FromDays(14);
        options.SlidingExpiration = true;
        options.Events.OnRedirectToLogin = context =>
        {
            if (context.Request.Path.StartsWithSegments("/api")) { context.Response.StatusCode = StatusCodes.Status401Unauthorized; return Task.CompletedTask; }
            context.Response.Redirect(context.RedirectUri);
            return Task.CompletedTask;
        };
    });
builder.Services.AddAuthorization();
builder.Services.AddRateLimiter(options =>
{
    options.AddPolicy("complaints", context => RateLimitPartition.GetFixedWindowLimiter(
        context.Connection.RemoteIpAddress?.ToString() ?? "unknown", _ => new FixedWindowRateLimiterOptions
        {
            PermitLimit = 5, Window = TimeSpan.FromMinutes(10), QueueLimit = 0, AutoReplenishment = true
        }));
    options.OnRejected = async (context, cancellationToken) =>
    {
        context.HttpContext.Response.StatusCode = StatusCodes.Status429TooManyRequests;
        context.HttpContext.Response.Headers.RetryAfter = "600";
        context.HttpContext.Response.ContentType = "text/plain; charset=utf-8";
        await context.HttpContext.Response.WriteAsync("تعداد درخواست‌ها بیش از حد مجاز است. لطفاً ۱۰ دقیقه دیگر دوباره تلاش کنید یا با فروشگاه تماس بگیرید.", cancellationToken);
    };
});
builder.Services.AddScoped<SeoCatalog>();
builder.Services.AddScoped<SeoResultFilter>();
builder.Services.AddControllersWithViews(options => options.Filters.AddService<SeoResultFilter>());
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
    await ArticleSeeder.SeedAsync(db, app.Environment);
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
app.UseRateLimiter();
app.MapControllers();
app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.Run();

