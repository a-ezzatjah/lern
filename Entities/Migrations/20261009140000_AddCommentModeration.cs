using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;

namespace Entities.Migrations;

[DbContext(typeof(ShopDbContext))]
[Migration("20261009140000_AddCommentModeration")]
public sealed class AddCommentModeration : Migration
{
    protected override void Up(MigrationBuilder migrationBuilder) => migrationBuilder.AddColumn<bool>(
        name: "IsBlocked", table: "ProductComments", type: "bit", nullable: false, defaultValue: false);

    protected override void Down(MigrationBuilder migrationBuilder) => migrationBuilder.DropColumn(
        name: "IsBlocked", table: "ProductComments");
}
