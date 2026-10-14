using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Entities.Migrations
{
    /// <inheritdoc />
    public partial class AddIntegrationProviderConfiguration : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "PaymentApiKeyHeader",
                table: "SiteIntegrationSettings",
                type: "nvarchar(100)",
                maxLength: 100,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "PaymentCheckoutUrlTemplate",
                table: "SiteIntegrationSettings",
                type: "nvarchar(500)",
                maxLength: 500,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "PaymentRequestUrl",
                table: "SiteIntegrationSettings",
                type: "nvarchar(500)",
                maxLength: 500,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "PaymentVerifyUrl",
                table: "SiteIntegrationSettings",
                type: "nvarchar(500)",
                maxLength: 500,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "SmsApiKeyHeader",
                table: "SiteIntegrationSettings",
                type: "nvarchar(100)",
                maxLength: 100,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "SmsApiUrl",
                table: "SiteIntegrationSettings",
                type: "nvarchar(500)",
                maxLength: 500,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "SmsMessageTemplate",
                table: "SiteIntegrationSettings",
                type: "nvarchar(500)",
                maxLength: 500,
                nullable: false,
                defaultValue: "");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "PaymentApiKeyHeader",
                table: "SiteIntegrationSettings");

            migrationBuilder.DropColumn(
                name: "PaymentCheckoutUrlTemplate",
                table: "SiteIntegrationSettings");

            migrationBuilder.DropColumn(
                name: "PaymentRequestUrl",
                table: "SiteIntegrationSettings");

            migrationBuilder.DropColumn(
                name: "PaymentVerifyUrl",
                table: "SiteIntegrationSettings");

            migrationBuilder.DropColumn(
                name: "SmsApiKeyHeader",
                table: "SiteIntegrationSettings");

            migrationBuilder.DropColumn(
                name: "SmsApiUrl",
                table: "SiteIntegrationSettings");

            migrationBuilder.DropColumn(
                name: "SmsMessageTemplate",
                table: "SiteIntegrationSettings");
        }
    }
}
