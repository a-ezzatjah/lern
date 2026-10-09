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
builder.Services.AddStoreRequestSecurity(builder.Environment.IsDevelopment());
builder.Services.AddDataProtection();
builder.Services.AddScoped<CustomerSession>();
builder.Services.AddScoped<OrderReservations>();
builder.Services.AddScoped<OrderPayments>();
builder.Services.AddScoped<IOrderPaymentGateway, UnavailablePaymentGateway>();
builder.Services.AddHostedService<OrderReservationWorker>();
builder.Services.AddScoped<IProductService, ProductService>();
builder.Services.AddScoped<ICategoryService, CategoryService>();
builder.Services.AddScoped<IProductSaleOptionService, ProductSaleOptionService>();
builder.Services.AddScoped<IProductSaleOptionColorService, ProductSaleOptionColorService>();
builder.Services.AddScoped<IProductVariantService, ProductVariantService>();
builder.Services.AddScoped<IPricingService, PricingService>();
builder.Services.AddScoped<IUserAuthService, UserAuthService>();
builder.Services.AddScoped<StoreCookieEvents>();
builder.Services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
    .AddCookie(options =>
    {
        options.LoginPath = "/login";
        options.AccessDeniedPath = "/account/access-denied";
        options.Cookie.Name = "lern.auth";
        options.Cookie.HttpOnly = true;
        options.Cookie.SecurePolicy = builder.Environment.IsDevelopment() ? CookieSecurePolicy.SameAsRequest : CookieSecurePolicy.Always;
        options.Cookie.Path = "/";
        options.Cookie.SameSite = SameSiteMode.Lax;
        options.Cookie.IsEssential = true;
        options.ExpireTimeSpan = TimeSpan.FromDays(14);
        options.SlidingExpiration = true;
        options.EventsType = typeof(StoreCookieEvents);
    });
builder.Services.AddAuthorization();
builder.Services.AddRateLimiter(options =>
{
    options.GlobalLimiter = StoreRequestSecurity.CreateAuthenticationLimiter();
    options.AddPolicy("complaints", context => RateLimitPartition.GetFixedWindowLimiter(
        context.Connection.RemoteIpAddress?.ToString() ?? "unknown", _ => new FixedWindowRateLimiterOptions
        {
            PermitLimit = 5, Window = TimeSpan.FromMinutes(10), QueueLimit = 0, AutoReplenishment = true
        }));
    options.OnRejected = StoreRequestSecurity.RejectRateLimit;
});
builder.Services.AddScoped<SeoCatalog>();
builder.Services.AddScoped<SeoResultFilter>();
builder.Services.AddControllersWithViews(options =>
{
    options.Filters.AddService<SeoResultFilter>();
    options.Filters.AddService<StoreWriteProtection>();
});
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
    if (builder.Configuration.GetValue<bool>("InitialAdmin:Enabled"))
    {
        var phone = builder.Configuration["InitialAdmin:PhoneNumber"] ?? "";
        app.Logger.LogWarning("{InitialAdminResult}", await InitialAdminSetup.PromoteAsync(db, phone));
    }
}
catch (Exception ex)
{
    app.Logger.LogWarning(ex, "اجرای migration های دیتابیس انجام نشد.");
}

app.UseStoreResponseSecurity();
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}
else
{
    app.UseExceptionHandler(error => error.Run(async context =>
    {
        context.Response.StatusCode = StatusCodes.Status500InternalServerError;
        context.Response.Headers.CacheControl = "no-store";
        await context.Response.WriteAsJsonAsync(new
        {
            message = "خطایی رخ داد. لطفاً دوباره تلاش کنید یا با پشتیبانی تماس بگیرید.",
            requestId = context.TraceIdentifier
        });
    }));
    app.UseHsts();
    app.UseHttpsRedirection();
}
app.UseStaticFiles();
app.UseAuthentication();
app.UseAuthorization();
app.UseRateLimiter();
app.MapControllers();
app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.Run();
