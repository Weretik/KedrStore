using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Content.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddContentEditors : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "UpdatedBy",
                schema: "content",
                table: "CompanyContacts",
                type: "character varying(256)",
                maxLength: 256,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "UpdatedBy",
                schema: "content",
                table: "Articles",
                type: "character varying(256)",
                maxLength: 256,
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "UpdatedBy",
                schema: "content",
                table: "CompanyContacts");

            migrationBuilder.DropColumn(
                name: "UpdatedBy",
                schema: "content",
                table: "Articles");
        }
    }
}
