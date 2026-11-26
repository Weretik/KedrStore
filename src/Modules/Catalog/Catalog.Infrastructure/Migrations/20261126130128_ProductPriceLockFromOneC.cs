using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Catalog.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class ProductPriceLockFromOneC : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<DateTimeOffset>(
                name: "LockedFromOneCAt",
                schema: "catalog",
                table: "ProductPrices",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<uint>(
                name: "xmin",
                schema: "catalog",
                table: "ProductPrices",
                type: "xid",
                rowVersion: true,
                nullable: false,
                defaultValue: 0u);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "LockedFromOneCAt",
                schema: "catalog",
                table: "ProductPrices");

            migrationBuilder.DropColumn(
                name: "xmin",
                schema: "catalog",
                table: "ProductPrices");
        }
    }
}
