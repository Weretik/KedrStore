using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Content.Infrastructure.Migrations
{
    /// <summary>Creates the one contacts row with the values the site kept in code until now.</summary>
    public partial class AddCompanyContacts : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "CompanyContacts",
                schema: "content",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false),
                    SalesPhone = table.Column<string>(type: "character varying(13)", maxLength: 13, nullable: false),
                    Email = table.Column<string>(type: "character varying(120)", maxLength: 120, nullable: false),
                    TelegramUrl = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: false),
                    ViberPhone = table.Column<string>(type: "character varying(13)", maxLength: 13, nullable: false),
                    MapUrl = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: false),
                    CityUk = table.Column<string>(type: "character varying(80)", maxLength: 80, nullable: false),
                    CityRu = table.Column<string>(type: "character varying(80)", maxLength: 80, nullable: false),
                    UpdatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CompanyContacts", x => x.Id);
                    table.CheckConstraint("CK_CompanyContacts_Singleton", "\"Id\" = 1");
                });

            migrationBuilder.InsertData(
                schema: "content",
                table: "CompanyContacts",
                columns: ["Id", "SalesPhone", "Email", "TelegramUrl", "ViberPhone", "MapUrl", "CityUk", "CityRu", "UpdatedAt"],
                values: new object[]
                {
                    1, "+380687135049", "kedr.class19@gmail.com", "https://t.me/+380687135049", "+380687135049",
                    "https://maps.app.goo.gl/D2rH3HcaxLfxik5H6", "Харків", "Харьков",
                    new DateTimeOffset(2026, 11, 4, 0, 0, 0, TimeSpan.Zero)
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "CompanyContacts",
                schema: "content");
        }
    }
}
