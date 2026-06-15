using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace Entities.Migrations
{
    /// <inheritdoc />
    public partial class addcategory : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_products_branches_BranchId",
                table: "products");

            migrationBuilder.DropTable(
                name: "branches");

            migrationBuilder.DropIndex(
                name: "IX_products_BranchId",
                table: "products");

            migrationBuilder.DeleteData(
                table: "products",
                keyColumn: "Id",
                keyValue: 1);

            migrationBuilder.DeleteData(
                table: "products",
                keyColumn: "Id",
                keyValue: 2);

            migrationBuilder.DeleteData(
                table: "products",
                keyColumn: "Id",
                keyValue: 3);

            migrationBuilder.DeleteData(
                table: "products",
                keyColumn: "Id",
                keyValue: 4);

            migrationBuilder.DeleteData(
                table: "products",
                keyColumn: "Id",
                keyValue: 5);

            migrationBuilder.DeleteData(
                table: "products",
                keyColumn: "Id",
                keyValue: 6);

            migrationBuilder.DropColumn(
                name: "BranchId",
                table: "products");

            migrationBuilder.CreateTable(
                name: "categories",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Name = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Slug = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    CategoryId = table.Column<int>(type: "int", nullable: true),
                    SortOrder = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_categories", x => x.Id);
                    table.ForeignKey(
                        name: "FK_categories_categories_CategoryId",
                        column: x => x.CategoryId,
                        principalTable: "categories",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateIndex(
                name: "IX_categories_CategoryId",
                table: "categories",
                column: "CategoryId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "categories");

            migrationBuilder.AddColumn<int>(
                name: "BranchId",
                table: "products",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.CreateTable(
                name: "branches",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Name = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false, defaultValue: "ونک")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_branches", x => x.Id);
                });

            migrationBuilder.InsertData(
                table: "branches",
                columns: new[] { "Id", "Name" },
                values: new object[,]
                {
                    { 1, "شهر آرا" },
                    { 2, "ونک" },
                    { 3, "سعادت آباد" }
                });

            migrationBuilder.InsertData(
                table: "products",
                columns: new[] { "Id", "BranchId", "Description", "DisconType", "Discount", "HasDiscount", "Name", "Price" },
                values: new object[,]
                {
                    { 1, 1, "core i7", 2, 200000m, true, "لپ تاب", 2000000m },
                    { 2, 2, "redmi note 14", 2, 200000m, true, "گوشی", 2000000m },
                    { 3, 1, "good", 2, 200000m, true, "موس", 700000m },
                    { 4, 3, "همراه اول ", 1, 10m, true, "مودم", 10000000m },
                    { 5, 1, "17 pro max ", 2, 200000m, true, "آیفون", 2000000m },
                    { 6, 2, "پرو", 1, 15m, true, "ps5", 100000000m }
                });

            migrationBuilder.CreateIndex(
                name: "IX_products_BranchId",
                table: "products",
                column: "BranchId");

            migrationBuilder.AddForeignKey(
                name: "FK_products_branches_BranchId",
                table: "products",
                column: "BranchId",
                principalTable: "branches",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
