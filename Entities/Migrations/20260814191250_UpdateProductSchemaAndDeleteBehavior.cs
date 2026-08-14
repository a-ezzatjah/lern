using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Entities.Migrations
{
    /// <inheritdoc />
    public partial class UpdateProductSchemaAndDeleteBehavior : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_ProductCategories_categories_CategoryId",
                table: "ProductCategories");

            migrationBuilder.DropForeignKey(
                name: "FK_ProductCategories_products_ProductId",
                table: "ProductCategories");

            migrationBuilder.DropForeignKey(
                name: "FK_ProductImages_ProductVariants_VariantId",
                table: "ProductImages");

            migrationBuilder.DropForeignKey(
                name: "FK_ProductImages_products_ProductId",
                table: "ProductImages");

            migrationBuilder.DropForeignKey(
                name: "FK_ProductSaleOptionColors_ProductSaleOptions_ProductSaleOptionId",
                table: "ProductSaleOptionColors");

            migrationBuilder.DropForeignKey(
                name: "FK_ProductVariants_ProductSaleOptionColors_ProductSaleOptionColorId",
                table: "ProductVariants");

            migrationBuilder.DropForeignKey(
                name: "FK_ProductVariants_products_ProductId",
                table: "ProductVariants");

            migrationBuilder.DropIndex(
                name: "IX_ProductVariants_ProductId",
                table: "ProductVariants");

            migrationBuilder.DropPrimaryKey(
                name: "PK_ProductSaleOptionColors",
                table: "ProductSaleOptionColors");

            migrationBuilder.DropPrimaryKey(
                name: "PK_ProductImages",
                table: "ProductImages");

            migrationBuilder.DropColumn(
                name: "MaxQuantity",
                table: "ProductVariants");

            migrationBuilder.DropColumn(
                name: "MinQuantity",
                table: "ProductVariants");

            migrationBuilder.DropColumn(
                name: "ProductId",
                table: "ProductVariants");

            migrationBuilder.DropColumn(
                name: "Step",
                table: "ProductVariants");

            migrationBuilder.DropColumn(
                name: "BasePrice",
                table: "ProductSaleOptions");

            migrationBuilder.DropColumn(
                name: "FixedHeight",
                table: "ProductSaleOptions");

            migrationBuilder.DropColumn(
                name: "FixedLength",
                table: "ProductSaleOptions");

            migrationBuilder.DropColumn(
                name: "FixedWeight",
                table: "ProductSaleOptions");

            migrationBuilder.DropColumn(
                name: "FixedWidth",
                table: "ProductSaleOptions");

            migrationBuilder.Sql(
                """
                IF COL_LENGTH(N'ProductSaleOptions', N'ImageUrl') IS NOT NULL
                    ALTER TABLE [ProductSaleOptions] DROP COLUMN [ImageUrl];
                """);

            migrationBuilder.DropColumn(
                name: "PerUnitHeight",
                table: "ProductSaleOptions");

            migrationBuilder.DropColumn(
                name: "PerUnitLength",
                table: "ProductSaleOptions");

            migrationBuilder.DropColumn(
                name: "PerUnitWeight",
                table: "ProductSaleOptions");

            migrationBuilder.DropColumn(
                name: "PerUnitWidth",
                table: "ProductSaleOptions");

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
                name: "OgDescription",
                table: "categories");

            migrationBuilder.DropColumn(
                name: "OgImageUrl",
                table: "categories");

            migrationBuilder.DropColumn(
                name: "OgTitle",
                table: "categories");

            migrationBuilder.Sql(
                """
                IF COL_LENGTH(N'ProductSaleOptionColors', N'ImageUrl') IS NOT NULL
                    ALTER TABLE [ProductSaleOptionColors] DROP COLUMN [ImageUrl];
                """);

            migrationBuilder.DropColumn(
                name: "Price",
                table: "ProductSaleOptionColors");

            migrationBuilder.RenameTable(
                name: "ProductSaleOptionColors",
                newName: "SaleOptionColors");

            migrationBuilder.RenameTable(
                name: "ProductImages",
                newName: "ProductImage");

            migrationBuilder.RenameIndex(
                name: "IX_ProductSaleOptionColors_ProductSaleOptionId",
                table: "SaleOptionColors",
                newName: "IX_SaleOptionColors_ProductSaleOptionId");

            migrationBuilder.RenameIndex(
                name: "IX_ProductImages_VariantId",
                table: "ProductImage",
                newName: "IX_ProductImage_VariantId");

            migrationBuilder.RenameIndex(
                name: "IX_ProductImages_ProductId",
                table: "ProductImage",
                newName: "IX_ProductImage_ProductId");

            migrationBuilder.AlterColumn<int>(
                name: "ProductSaleOptionColorId",
                table: "ProductVariants",
                type: "int",
                nullable: true,
                oldClrType: typeof(int),
                oldType: "int");

            migrationBuilder.AlterColumn<int>(
                name: "Step",
                table: "ProductSaleOptions",
                type: "int",
                precision: 18,
                scale: 3,
                nullable: false,
                oldClrType: typeof(decimal),
                oldType: "decimal(18,3)",
                oldPrecision: 18,
                oldScale: 3);

            migrationBuilder.AlterColumn<int>(
                name: "MinQuantity",
                table: "ProductSaleOptions",
                type: "int",
                precision: 18,
                scale: 3,
                nullable: true,
                oldClrType: typeof(decimal),
                oldType: "decimal(18,3)",
                oldPrecision: 18,
                oldScale: 3,
                oldNullable: true);

            migrationBuilder.AlterColumn<int>(
                name: "MaxQuantity",
                table: "ProductSaleOptions",
                type: "int",
                precision: 18,
                scale: 3,
                nullable: true,
                oldClrType: typeof(decimal),
                oldType: "decimal(18,3)",
                oldPrecision: 18,
                oldScale: 3,
                oldNullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ShortDescription",
                table: "products",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "ImageUrl",
                table: "ProductImage",
                type: "nvarchar(max)",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)");

            migrationBuilder.AlterColumn<string>(
                name: "AltText",
                table: "ProductImage",
                type: "nvarchar(max)",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)");

            migrationBuilder.AddPrimaryKey(
                name: "PK_SaleOptionColors",
                table: "SaleOptionColors",
                column: "Id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_ProductImage",
                table: "ProductImage",
                column: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_ProductCategories_categories_CategoryId",
                table: "ProductCategories",
                column: "CategoryId",
                principalTable: "categories",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_ProductCategories_products_ProductId",
                table: "ProductCategories",
                column: "ProductId",
                principalTable: "products",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_ProductImage_ProductVariants_VariantId",
                table: "ProductImage",
                column: "VariantId",
                principalTable: "ProductVariants",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_ProductImage_products_ProductId",
                table: "ProductImage",
                column: "ProductId",
                principalTable: "products",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_ProductVariants_SaleOptionColors_ProductSaleOptionColorId",
                table: "ProductVariants",
                column: "ProductSaleOptionColorId",
                principalTable: "SaleOptionColors",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_SaleOptionColors_ProductSaleOptions_ProductSaleOptionId",
                table: "SaleOptionColors",
                column: "ProductSaleOptionId",
                principalTable: "ProductSaleOptions",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_ProductCategories_categories_CategoryId",
                table: "ProductCategories");

            migrationBuilder.DropForeignKey(
                name: "FK_ProductCategories_products_ProductId",
                table: "ProductCategories");

            migrationBuilder.DropForeignKey(
                name: "FK_ProductImage_ProductVariants_VariantId",
                table: "ProductImage");

            migrationBuilder.DropForeignKey(
                name: "FK_ProductImage_products_ProductId",
                table: "ProductImage");

            migrationBuilder.DropForeignKey(
                name: "FK_ProductVariants_SaleOptionColors_ProductSaleOptionColorId",
                table: "ProductVariants");

            migrationBuilder.DropForeignKey(
                name: "FK_SaleOptionColors_ProductSaleOptions_ProductSaleOptionId",
                table: "SaleOptionColors");

            migrationBuilder.DropPrimaryKey(
                name: "PK_SaleOptionColors",
                table: "SaleOptionColors");

            migrationBuilder.DropPrimaryKey(
                name: "PK_ProductImage",
                table: "ProductImage");

            migrationBuilder.DropColumn(
                name: "ShortDescription",
                table: "products");

            migrationBuilder.RenameTable(
                name: "SaleOptionColors",
                newName: "ProductSaleOptionColors");

            migrationBuilder.RenameTable(
                name: "ProductImage",
                newName: "ProductImages");

            migrationBuilder.RenameIndex(
                name: "IX_SaleOptionColors_ProductSaleOptionId",
                table: "ProductSaleOptionColors",
                newName: "IX_ProductSaleOptionColors_ProductSaleOptionId");

            migrationBuilder.RenameIndex(
                name: "IX_ProductImage_VariantId",
                table: "ProductImages",
                newName: "IX_ProductImages_VariantId");

            migrationBuilder.RenameIndex(
                name: "IX_ProductImage_ProductId",
                table: "ProductImages",
                newName: "IX_ProductImages_ProductId");

            migrationBuilder.AlterColumn<int>(
                name: "ProductSaleOptionColorId",
                table: "ProductVariants",
                type: "int",
                nullable: false,
                defaultValue: 0,
                oldClrType: typeof(int),
                oldType: "int",
                oldNullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "MaxQuantity",
                table: "ProductVariants",
                type: "decimal(18,3)",
                precision: 18,
                scale: 3,
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "MinQuantity",
                table: "ProductVariants",
                type: "decimal(18,3)",
                precision: 18,
                scale: 3,
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "ProductId",
                table: "ProductVariants",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<decimal>(
                name: "Step",
                table: "ProductVariants",
                type: "decimal(18,3)",
                precision: 18,
                scale: 3,
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AlterColumn<decimal>(
                name: "Step",
                table: "ProductSaleOptions",
                type: "decimal(18,3)",
                precision: 18,
                scale: 3,
                nullable: false,
                oldClrType: typeof(int),
                oldType: "int",
                oldPrecision: 18,
                oldScale: 3);

            migrationBuilder.AlterColumn<decimal>(
                name: "MinQuantity",
                table: "ProductSaleOptions",
                type: "decimal(18,3)",
                precision: 18,
                scale: 3,
                nullable: true,
                oldClrType: typeof(int),
                oldType: "int",
                oldPrecision: 18,
                oldScale: 3,
                oldNullable: true);

            migrationBuilder.AlterColumn<decimal>(
                name: "MaxQuantity",
                table: "ProductSaleOptions",
                type: "decimal(18,3)",
                precision: 18,
                scale: 3,
                nullable: true,
                oldClrType: typeof(int),
                oldType: "int",
                oldPrecision: 18,
                oldScale: 3,
                oldNullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "BasePrice",
                table: "ProductSaleOptions",
                type: "decimal(18,2)",
                precision: 18,
                scale: 2,
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "FixedHeight",
                table: "ProductSaleOptions",
                type: "decimal(18,3)",
                precision: 18,
                scale: 3,
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "FixedLength",
                table: "ProductSaleOptions",
                type: "decimal(18,3)",
                precision: 18,
                scale: 3,
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "FixedWeight",
                table: "ProductSaleOptions",
                type: "decimal(18,3)",
                precision: 18,
                scale: 3,
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "FixedWidth",
                table: "ProductSaleOptions",
                type: "decimal(18,3)",
                precision: 18,
                scale: 3,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ImageUrl",
                table: "ProductSaleOptions",
                type: "nvarchar(500)",
                maxLength: 500,
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "PerUnitHeight",
                table: "ProductSaleOptions",
                type: "decimal(18,3)",
                precision: 18,
                scale: 3,
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "PerUnitLength",
                table: "ProductSaleOptions",
                type: "decimal(18,3)",
                precision: 18,
                scale: 3,
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "PerUnitWeight",
                table: "ProductSaleOptions",
                type: "decimal(18,3)",
                precision: 18,
                scale: 3,
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "PerUnitWidth",
                table: "ProductSaleOptions",
                type: "decimal(18,3)",
                precision: 18,
                scale: 3,
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

            migrationBuilder.AddColumn<string>(
                name: "ImageUrl",
                table: "ProductSaleOptionColors",
                type: "nvarchar(500)",
                maxLength: 500,
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "Price",
                table: "ProductSaleOptionColors",
                type: "decimal(18,2)",
                precision: 18,
                scale: 2,
                nullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "ImageUrl",
                table: "ProductImages",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "nvarchar(max)",
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "AltText",
                table: "ProductImages",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "nvarchar(max)",
                oldNullable: true);

            migrationBuilder.AddPrimaryKey(
                name: "PK_ProductSaleOptionColors",
                table: "ProductSaleOptionColors",
                column: "Id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_ProductImages",
                table: "ProductImages",
                column: "Id");

            migrationBuilder.CreateIndex(
                name: "IX_ProductVariants_ProductId",
                table: "ProductVariants",
                column: "ProductId");

            migrationBuilder.AddForeignKey(
                name: "FK_ProductCategories_categories_CategoryId",
                table: "ProductCategories",
                column: "CategoryId",
                principalTable: "categories",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_ProductCategories_products_ProductId",
                table: "ProductCategories",
                column: "ProductId",
                principalTable: "products",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_ProductImages_ProductVariants_VariantId",
                table: "ProductImages",
                column: "VariantId",
                principalTable: "ProductVariants",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_ProductImages_products_ProductId",
                table: "ProductImages",
                column: "ProductId",
                principalTable: "products",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_ProductSaleOptionColors_ProductSaleOptions_ProductSaleOptionId",
                table: "ProductSaleOptionColors",
                column: "ProductSaleOptionId",
                principalTable: "ProductSaleOptions",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_ProductVariants_ProductSaleOptionColors_ProductSaleOptionColorId",
                table: "ProductVariants",
                column: "ProductSaleOptionColorId",
                principalTable: "ProductSaleOptionColors",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_ProductVariants_products_ProductId",
                table: "ProductVariants",
                column: "ProductId",
                principalTable: "products",
                principalColumn: "Id");
        }
    }
}
