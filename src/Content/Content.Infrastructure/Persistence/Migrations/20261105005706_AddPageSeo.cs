using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Content.Infrastructure.Persistence.Migrations
{
    /// <summary>Adds editor-set search titles and descriptions of the site pages; no rows until the first save.</summary>
    public partial class AddPageSeo : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "PageSeo",
                schema: "content",
                columns: table => new
                {
                    Page = table.Column<string>(type: "character varying(32)", maxLength: 32, nullable: false),
                    TitleUk = table.Column<string>(type: "character varying(120)", maxLength: 120, nullable: true),
                    TitleRu = table.Column<string>(type: "character varying(120)", maxLength: 120, nullable: true),
                    DescriptionUk = table.Column<string>(type: "character varying(300)", maxLength: 300, nullable: true),
                    DescriptionRu = table.Column<string>(type: "character varying(300)", maxLength: 300, nullable: true),
                    UpdatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    UpdatedBy = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PageSeo", x => x.Page);
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "PageSeo",
                schema: "content");
        }
    }
}
