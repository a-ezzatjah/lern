using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Entities.Migrations
{
    /// <inheritdoc />
    public partial class RenameSaleOptionColorToProductSaleOptionColor : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_SaleOptionColors_ProductSaleOptions_SaleOptionId",
                table: "SaleOptionColors");

            migrationBuilder.DropPrimaryKey(
                name: "PK_SaleOptionColors",
                table: "SaleOptionColors");

            migrationBuilder.RenameTable(
                name: "SaleOptionColors",
                newName: "ProductSaleOptionColors");

            migrationBuilder.RenameColumn(
                name: "SaleOptionId",
                table: "ProductSaleOptionColors",
                newName: "ProductSaleOptionId");

            migrationBuilder.RenameIndex(
                name: "IX_SaleOptionColors_SaleOptionId",
                table: "ProductSaleOptionColors",
                newName: "IX_ProductSaleOptionColors_ProductSaleOptionId");

            migrationBuilder.AddColumn<DateTime>(
                name: "DiscountEndAt",
                table: "products",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "DiscountStartAt",
                table: "products",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddPrimaryKey(
                name: "PK_ProductSaleOptionColors",
                table: "ProductSaleOptionColors",
                column: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_ProductSaleOptionColors_ProductSaleOptions_ProductSaleOptionId",
                table: "ProductSaleOptionColors",
                column: "ProductSaleOptionId",
                principalTable: "ProductSaleOptions",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.CreateTable(
                name: "ProductVariants",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ProductId = table.Column<int>(type: "int", nullable: false),
                    ProductSaleOptionId = table.Column<int>(type: "int", nullable: false),
                    ProductSaleOptionColorId = table.Column<int>(type: "int", nullable: false),
                    Sku = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Price = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false),
                    DiscountValue = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: true),
                    DisconType = table.Column<int>(type: "int", nullable: true),
                    DiscountStartAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    DiscountEndAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    StockQuantity = table.Column<int>(type: "int", nullable: false),
                    ReservedQuantity = table.Column<int>(type: "int", nullable: false),
                    MinQuantity = table.Column<decimal>(type: "decimal(18,3)", precision: 18, scale: 3, nullable: true),
                    MaxQuantity = table.Column<decimal>(type: "decimal(18,3)", precision: 18, scale: 3, nullable: true),
                    Step = table.Column<decimal>(type: "decimal(18,3)", precision: 18, scale: 3, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ProductVariants", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ProductVariants_ProductSaleOptionColors_ProductSaleOptionColorId",
                        column: x => x.ProductSaleOptionColorId,
                        principalTable: "ProductSaleOptionColors",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_ProductVariants_ProductSaleOptions_ProductSaleOptionId",
                        column: x => x.ProductSaleOptionId,
                        principalTable: "ProductSaleOptions",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_ProductVariants_products_ProductId",
                        column: x => x.ProductId,
                        principalTable: "products",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "ProductImages",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ProductId = table.Column<int>(type: "int", nullable: true),
                    VariantId = table.Column<int>(type: "int", nullable: true),
                    ImageUrl = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    AltText = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    SortOrder = table.Column<int>(type: "int", nullable: false),
                    IsPrimary = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ProductImages", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ProductImages_ProductVariants_VariantId",
                        column: x => x.VariantId,
                        principalTable: "ProductVariants",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_ProductImages_products_ProductId",
                        column: x => x.ProductId,
                        principalTable: "products",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateIndex(
                name: "IX_ProductImages_ProductId",
                table: "ProductImages",
                column: "ProductId");

            migrationBuilder.CreateIndex(
                name: "IX_ProductImages_VariantId",
                table: "ProductImages",
                column: "VariantId");

            migrationBuilder.CreateIndex(
                name: "IX_ProductVariants_ProductId",
                table: "ProductVariants",
                column: "ProductId");

            migrationBuilder.CreateIndex(
                name: "IX_ProductVariants_ProductSaleOptionColorId",
                table: "ProductVariants",
                column: "ProductSaleOptionColorId");

            migrationBuilder.CreateIndex(
                name: "IX_ProductVariants_ProductSaleOptionId",
                table: "ProductVariants",
                column: "ProductSaleOptionId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ProductImages");

            migrationBuilder.DropTable(
                name: "ProductVariants");

            migrationBuilder.DropForeignKey(
                name: "FK_ProductSaleOptionColors_ProductSaleOptions_ProductSaleOptionId",
                table: "ProductSaleOptionColors");

            migrationBuilder.DropPrimaryKey(
                name: "PK_ProductSaleOptionColors",
                table: "ProductSaleOptionColors");

            migrationBuilder.DropColumn(
                name: "DiscountEndAt",
                table: "products");

            migrationBuilder.DropColumn(
                name: "DiscountStartAt",
                table: "products");

            migrationBuilder.RenameIndex(
                name: "IX_ProductSaleOptionColors_ProductSaleOptionId",
                table: "ProductSaleOptionColors",
                newName: "IX_SaleOptionColors_SaleOptionId");

            migrationBuilder.RenameColumn(
                name: "ProductSaleOptionId",
                table: "ProductSaleOptionColors",
                newName: "SaleOptionId");

            migrationBuilder.RenameTable(
                name: "ProductSaleOptionColors",
                newName: "SaleOptionColors");

            migrationBuilder.AddPrimaryKey(
                name: "PK_SaleOptionColors",
                table: "SaleOptionColors",
                column: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_SaleOptionColors_ProductSaleOptions_SaleOptionId",
                table: "SaleOptionColors",
                column: "SaleOptionId",
                principalTable: "ProductSaleOptions",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
