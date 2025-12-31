using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Infrastructure.Catalog.Migrations
{
    /// <inheritdoc />
    public partial class AddQuantityInPacktoaproducttable : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "QuantityInPack",
                table: "Products",
                type: "integer",
                nullable: false,
                defaultValue: 0);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "QuantityInPack",
                table: "Products");
        }
    }
}
