using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Content.Infrastructure.Migrations
{
    /// <summary>Adds the site-wide SEO codes with one empty row: the site adds nothing until an editor saves.</summary>
    public partial class AddSiteSeoSettings : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "SiteSeoSettings",
                schema: "content",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false),
                    GoogleAnalyticsId = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: true),
                    GoogleTagManagerId = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: true),
                    GoogleSiteVerification = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    BingSiteVerification = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    ShareImageMediaFileId = table.Column<long>(type: "bigint", nullable: true),
                    UpdatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    UpdatedBy = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SiteSeoSettings", x => x.Id);
                    table.CheckConstraint("CK_SiteSeoSettings_Singleton", "\"Id\" = 1");
                });

            migrationBuilder.InsertData(
                schema: "content",
                table: "SiteSeoSettings",
                columns: ["Id", "UpdatedAt"],
                values: new object[] { 1, new DateTimeOffset(2026, 11, 12, 0, 0, 0, TimeSpan.Zero) });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "SiteSeoSettings",
                schema: "content");
        }
    }
}
