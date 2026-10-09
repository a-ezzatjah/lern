using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Entities.Migrations;

[DbContext(typeof(ShopDbContext))]
[Migration("20261009123000_CorrectElasticCordCategoryName")]
public sealed class CorrectElasticCordCategoryName : Migration
{
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.Sql("UPDATE [categories] SET [Name] = N'کش قیطان' WHERE [Slug] = N'elastic-qizan' AND [Name] = N'کش قیزان';");
    }

    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.Sql("UPDATE [categories] SET [Name] = N'کش قیزان' WHERE [Slug] = N'elastic-qizan' AND [Name] = N'کش قیطان';");
    }
}
