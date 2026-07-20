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


var builder = WebApplication.CreateBuilder(args);


builder.Services.AddMemoryCache();
builder.Services.AddScoped<IProductService, ProductService>();
builder.Services.AddScoped<ICategoryService, CategoryService>();
builder.Services.AddScoped<IProductSaleOptionService, ProductSaleOptionService>();
builder.Services.AddScoped<IProductSaleOptionColorService, ProductSaleOptionColorService>();
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddAutoMapper(cfg =>
{
    cfg.AddProfile<ProductMappingProfile>();
    cfg.AddProfile<CategoryMappingProfile>();
});
builder.Services.AddDbContext<ShopDbContext>(option => option.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnectionstring")));
builder.Services.AddValidatorsFromAssemblyContaining<ProductCreateDtoValidator>();

var app = builder.Build();

app.UseSwagger();
app.UseSwaggerUI();
app.MapControllers();

app.Run();

