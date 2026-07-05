using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Entities.Migrations
{
    /// <inheritdoc />
    public partial class shop3 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ProductSeoData");

            migrationBuilder.DropColumn(
                name: "Price",
                table: "products");

            migrationBuilder.RenameColumn(
                name: "HasDiscount",
                table: "products",
                newName: "IndexPage");

            migrationBuilder.AddColumn<string>(
                name: "HexCode",
                table: "SaleOptionColors",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "BasePrice",
                table: "ProductSaleOptions",
                type: "decimal(18,2)",
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<string>(
                name: "CanonicalUrl",
                table: "products",
                type: "nvarchar(500)",
                maxLength: 500,
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "FollowPage",
                table: "products",
                type: "bit",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "MetaDescription",
                table: "products",
                type: "nvarchar(500)",
                maxLength: 500,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "MetaKeywords",
                table: "products",
                type: "nvarchar(300)",
                maxLength: 300,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "MetaTitle",
                table: "products",
                type: "nvarchar(200)",
                maxLength: 200,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "OgDescription",
                table: "products",
                type: "nvarchar(500)",
                maxLength: 500,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "OgImageUrl",
                table: "products",
                type: "nvarchar(500)",
                maxLength: 500,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "OgTitle",
                table: "products",
                type: "nvarchar(200)",
                maxLength: 200,
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "Seo_CreatedAt",
                table: "products",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "Seo_UpdatedAt",
                table: "products",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "CanonicalUrl",
                table: "categories",
                type: "nvarchar(500)",
                maxLength: 500,
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "FollowPage",
                table: "categories",
                type: "bit",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "IndexPage",
                table: "categories",
                type: "bit",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "MetaDescription",
                table: "categories",
                type: "nvarchar(500)",
                maxLength: 500,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "MetaKeywords",
                table: "categories",
                type: "nvarchar(300)",
                maxLength: 300,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "MetaTitle",
                table: "categories",
                type: "nvarchar(200)",
                maxLength: 200,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "OgDescription",
                table: "categories",
                type: "nvarchar(500)",
                maxLength: 500,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "OgImageUrl",
                table: "categories",
                type: "nvarchar(500)",
                maxLength: 500,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "OgTitle",
                table: "categories",
                type: "nvarchar(200)",
                maxLength: 200,
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "Seo_CreatedAt",
                table: "categories",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "Seo_UpdatedAt",
                table: "categories",
                type: "datetime2",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "HexCode",
                table: "SaleOptionColors");

            migrationBuilder.DropColumn(
                name: "BasePrice",
                table: "ProductSaleOptions");

            migrationBuilder.DropColumn(
                name: "CanonicalUrl",
                table: "products");

            migrationBuilder.DropColumn(
                name: "FollowPage",
                table: "products");

            migrationBuilder.DropColumn(
                name: "MetaDescription",
                table: "products");

            migrationBuilder.DropColumn(
                name: "MetaKeywords",
                table: "products");

            migrationBuilder.DropColumn(
                name: "MetaTitle",
                table: "products");

            migrationBuilder.DropColumn(
                name: "OgDescription",
                table: "products");

            migrationBuilder.DropColumn(
                name: "OgImageUrl",
                table: "products");

            migrationBuilder.DropColumn(
                name: "OgTitle",
                table: "products");

            migrationBuilder.DropColumn(
                name: "Seo_CreatedAt",
                table: "products");

            migrationBuilder.DropColumn(
                name: "Seo_UpdatedAt",
                table: "products");

            migrationBuilder.DropColumn(
                name: "CanonicalUrl",
                table: "categories");

            migrationBuilder.DropColumn(
                name: "FollowPage",
                table: "categories");

            migrationBuilder.DropColumn(
                name: "IndexPage",
                table: "categories");

            migrationBuilder.DropColumn(
                name: "MetaDescription",
                table: "categories");

            migrationBuilder.DropColumn(
                name: "MetaKeywords",
                table: "categories");

            migrationBuilder.DropColumn(
                name: "MetaTitle",
                table: "categories");

            migrationBuilder.DropColumn(
                name: "OgDescription",
                table: "categories");

            migrationBuilder.DropColumn(
                name: "OgImageUrl",
                table: "categories");

            migrationBuilder.DropColumn(
                name: "OgTitle",
                table: "categories");

            migrationBuilder.DropColumn(
                name: "Seo_CreatedAt",
                table: "categories");

            migrationBuilder.DropColumn(
                name: "Seo_UpdatedAt",
                table: "categories");

            migrationBuilder.RenameColumn(
                name: "IndexPage",
                table: "products",
                newName: "HasDiscount");

            migrationBuilder.AddColumn<decimal>(
                name: "Price",
                table: "products",
                type: "decimal(18,2)",
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.CreateTable(
                name: "ProductSeoData",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false),
                    CanonicalUrl = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    FollowPage = table.Column<bool>(type: "bit", nullable: false),
                    IndexPage = table.Column<bool>(type: "bit", nullable: false),
                    MetaDescription = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    MetaKeywords = table.Column<string>(type: "nvarchar(300)", maxLength: 300, nullable: true),
                    MetaTitle = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true),
                    OgDescription = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    OgImageUrl = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    OgTitle = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ProductSeoData", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ProductSeoData_products_Id",
                        column: x => x.Id,
                        principalTable: "products",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });
        }
    }
}
