using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Entities.Migrations;

[Migration("20260908120000_AddAddresses")]
public partial class AddAddresses : Migration
{
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.CreateTable(
            name: "Addresses",
            columns: table => new
            {
                Id = table.Column<int>(type: "int", nullable: false).Annotation("SqlServer:Identity", "1, 1"),
                CustomerKey = table.Column<string>(type: "nvarchar(450)", nullable: false),
                Title = table.Column<string>(type: "nvarchar(max)", nullable: false),
                Province = table.Column<string>(type: "nvarchar(max)", nullable: false),
                City = table.Column<string>(type: "nvarchar(max)", nullable: false),
                Details = table.Column<string>(type: "nvarchar(max)", nullable: false),
                PostalCode = table.Column<string>(type: "nvarchar(max)", nullable: false),
                Phone = table.Column<string>(type: "nvarchar(max)", nullable: false),
                ReceiverName = table.Column<string>(type: "nvarchar(max)", nullable: false),
                IsDefault = table.Column<bool>(type: "bit", nullable: false),
                UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: false)
            }, constraints: table => table.PrimaryKey("PK_Addresses", x => x.Id));
        migrationBuilder.CreateIndex(name: "IX_Addresses_CustomerKey_Title", table: "Addresses", columns: new[] { "CustomerKey", "Title" });
    }

    protected override void Down(MigrationBuilder migrationBuilder) => migrationBuilder.DropTable(name: "Addresses");
}
