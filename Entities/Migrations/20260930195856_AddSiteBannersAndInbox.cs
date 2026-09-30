using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Entities.Migrations
{
    /// <inheritdoc />
    public partial class AddSiteBannersAndInbox : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "SiteBanners",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ImageUrl = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    LinkUrl = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    AltText = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    SortOrder = table.Column<int>(type: "int", nullable: false),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SiteBanners", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "SiteInboxItems",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Kind = table.Column<int>(type: "int", nullable: false),
                    Title = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Body = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    RecipientUserId = table.Column<int>(type: "int", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SiteInboxItems", x => x.Id);
                    table.ForeignKey(
                        name: "FK_SiteInboxItems_Users_RecipientUserId",
                        column: x => x.RecipientUserId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_SiteBanners_IsActive_SortOrder",
                table: "SiteBanners",
                columns: new[] { "IsActive", "SortOrder" });

            migrationBuilder.CreateIndex(
                name: "IX_SiteInboxItems_RecipientUserId_CreatedAt",
                table: "SiteInboxItems",
                columns: new[] { "RecipientUserId", "CreatedAt" });

            migrationBuilder.InsertData("SiteBanners", new[] { "ImageUrl", "AltText", "SortOrder", "IsActive", "CreatedAt" },
                new object[,]
                {
                    { "/assets/images/slider/slider-2-1.jpg", "بنر فروشگاه ۱", 1, true, DateTime.UtcNow },
                    { "/assets/images/slider/slider-2-2.jpg", "بنر فروشگاه ۲", 2, true, DateTime.UtcNow },
                    { "/assets/images/slider/slider-2-3.jpg", "بنر فروشگاه ۳", 3, true, DateTime.UtcNow }
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "SiteBanners");

            migrationBuilder.DropTable(
                name: "SiteInboxItems");
        }
    }
}
