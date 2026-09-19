using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Entities.Migrations
{
    /// <inheritdoc />
    public partial class AllowMultipleProductComments : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_ProductComments_CustomerKey_ProductId",
                table: "ProductComments");

            migrationBuilder.CreateIndex(
                name: "IX_ProductComments_CustomerKey_ProductId",
                table: "ProductComments",
                columns: new[] { "CustomerKey", "ProductId" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_ProductComments_CustomerKey_ProductId",
                table: "ProductComments");

            migrationBuilder.CreateIndex(
                name: "IX_ProductComments_CustomerKey_ProductId",
                table: "ProductComments",
                columns: new[] { "CustomerKey", "ProductId" },
                unique: true);
        }
    }
}
