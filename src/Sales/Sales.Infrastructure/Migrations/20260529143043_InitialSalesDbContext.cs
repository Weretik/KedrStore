using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Sales.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class InitialSalesDbContext : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Counterparties",
                columns: table => new
                {
                    Id = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: false),
                    IdentityUserId = table.Column<Guid>(type: "uuid", nullable: false),
                    Name = table.Column<string>(type: "character varying(300)", maxLength: 300, nullable: false),
                    Email = table.Column<string>(type: "character varying(320)", maxLength: 320, nullable: false),
                    Phone = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    DefaultPriceTypeId = table.Column<int>(type: "integer", nullable: false),
                    CreatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    IsDeleted = table.Column<bool>(type: "boolean", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Counterparties", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "CounterpartyCategoryPriceTypes",
                columns: table => new
                {
                    CounterpartyId = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: false),
                    CategoryId = table.Column<int>(type: "integer", nullable: false),
                    PriceTypeId = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CounterpartyCategoryPriceTypes", x => new { x.CounterpartyId, x.CategoryId });
                });

            migrationBuilder.CreateIndex(
                name: "IX_Counterparties_DefaultPriceTypeId",
                table: "Counterparties",
                column: "DefaultPriceTypeId");

            migrationBuilder.CreateIndex(
                name: "IX_Counterparties_Email",
                table: "Counterparties",
                column: "Email");

            migrationBuilder.CreateIndex(
                name: "IX_Counterparties_IdentityUserId",
                table: "Counterparties",
                column: "IdentityUserId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_CounterpartyCategoryPriceTypes_CategoryId",
                table: "CounterpartyCategoryPriceTypes",
                column: "CategoryId");

            migrationBuilder.CreateIndex(
                name: "IX_CounterpartyCategoryPriceTypes_PriceTypeId",
                table: "CounterpartyCategoryPriceTypes",
                column: "PriceTypeId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Counterparties");

            migrationBuilder.DropTable(
                name: "CounterpartyCategoryPriceTypes");
        }
    }
}
