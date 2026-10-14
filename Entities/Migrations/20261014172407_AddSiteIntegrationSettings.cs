using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Entities.Migrations
{
    /// <inheritdoc />
    public partial class AddSiteIntegrationSettings : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "SiteIntegrationSettings",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    SmsEnabled = table.Column<bool>(type: "bit", nullable: false),
                    SmsProvider = table.Column<string>(type: "nvarchar(40)", maxLength: 40, nullable: false),
                    SmsApiKeyProtected = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    SmsSender = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    PaymentEnabled = table.Column<bool>(type: "bit", nullable: false),
                    PaymentGateway = table.Column<string>(type: "nvarchar(40)", maxLength: 40, nullable: false),
                    PaymentMerchantIdProtected = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    EnamadEnabled = table.Column<bool>(type: "bit", nullable: false),
                    EnamadCode = table.Column<string>(type: "nvarchar(4000)", maxLength: 4000, nullable: true),
                    EnamadLogoUrl = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SiteIntegrationSettings", x => x.Id);
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "SiteIntegrationSettings");
        }
    }
}
