using Microsoft.EntityFrameworkCore.Migrations;
#nullable disable
namespace Entities.Migrations;
[Migration("20260908130000_AddCustomerCheckoutInfo")]
public partial class AddCustomerCheckoutInfo : Migration
{
    protected override void Up(MigrationBuilder m)
    {
        m.AddColumn<string>("CustomerFirstName", "Orders", "nvarchar(max)", nullable: false, defaultValue: "");
        m.AddColumn<string>("CustomerLastName", "Orders", "nvarchar(max)", nullable: false, defaultValue: "");
        m.AddColumn<string>("CustomerPhone", "Orders", "nvarchar(max)", nullable: false, defaultValue: "");
    }
    protected override void Down(MigrationBuilder m)
    {
        m.DropColumn("CustomerFirstName", "Orders"); m.DropColumn("CustomerLastName", "Orders"); m.DropColumn("CustomerPhone", "Orders");
    }
}
