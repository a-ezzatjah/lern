using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace Entities.Migrations
{
    /// <inheritdoc />
    public partial class second : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
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
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData(
                table: "branches",
                keyColumn: "Id",
                keyValue: 1);

            migrationBuilder.DeleteData(
                table: "branches",
                keyColumn: "Id",
                keyValue: 2);

            migrationBuilder.DeleteData(
                table: "branches",
                keyColumn: "Id",
                keyValue: 3);

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
        }
    }
}
