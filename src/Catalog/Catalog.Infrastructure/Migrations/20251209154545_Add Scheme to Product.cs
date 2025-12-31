using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Infrastructure.Catalog.Migrations
{
    /// <inheritdoc />
    public partial class AddSchemetoProduct : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "Sсheme",
                table: "Products",
                type: "character varying(1000)",
                maxLength: 1000,
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Sсheme",
                table: "Products");
        }
    }
}
