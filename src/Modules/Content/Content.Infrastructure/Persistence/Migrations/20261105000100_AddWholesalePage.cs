using Content.Infrastructure.Persistence.Seed;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Content.Infrastructure.Persistence.Migrations
{
    /// <summary>Adds the editable page «Співпраця» with the texts the site kept in code.</summary>
    public partial class AddWholesalePage : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "WholesalePages",
                schema: "content",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false),
                    UpdatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    UpdatedBy = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: true),
                    Ru = table.Column<string>(type: "jsonb", nullable: false),
                    Uk = table.Column<string>(type: "jsonb", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_WholesalePages", x => x.Id);
                    table.CheckConstraint("CK_WholesalePages_Singleton", "\"Id\" = 1");
                });

            LegacyWholesaleSeed.Up(migrationBuilder);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "WholesalePages",
                schema: "content");
        }
    }
}
