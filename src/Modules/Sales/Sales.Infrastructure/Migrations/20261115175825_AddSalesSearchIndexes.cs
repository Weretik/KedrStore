using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Sales.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddSalesSearchIndexes : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Orders_CounterpartyId",
                table: "Orders");

            migrationBuilder.AlterDatabase()
                .Annotation("Npgsql:PostgresExtension:pg_trgm", ",,");

            migrationBuilder.CreateIndex(
                name: "IX_Orders_CounterpartyId_CreatedAt_Id",
                table: "Orders",
                columns: new[] { "CounterpartyId", "CreatedAt", "Id" });

            migrationBuilder.CreateIndex(
                name: "IX_Orders_CreatedAt_Id",
                table: "Orders",
                columns: new[] { "CreatedAt", "Id" });

            migrationBuilder.CreateIndex(
                name: "IX_Orders_OrderNumber_trgm",
                table: "Orders",
                column: "OrderNumber")
                .Annotation("Npgsql:IndexMethod", "gin")
                .Annotation("Npgsql:IndexOperators", new[] { "gin_trgm_ops" });

            migrationBuilder.CreateIndex(
                name: "IX_OrderDrafts_Title_trgm",
                table: "OrderDrafts",
                column: "Title")
                .Annotation("Npgsql:IndexMethod", "gin")
                .Annotation("Npgsql:IndexOperators", new[] { "gin_trgm_ops" });

            migrationBuilder.CreateIndex(
                name: "IX_Counterparties_Id_trgm",
                table: "Counterparties",
                column: "Id")
                .Annotation("Npgsql:IndexMethod", "gin")
                .Annotation("Npgsql:IndexOperators", new[] { "gin_trgm_ops" });

            migrationBuilder.CreateIndex(
                name: "IX_Counterparties_Name_trgm",
                table: "Counterparties",
                column: "Name")
                .Annotation("Npgsql:IndexMethod", "gin")
                .Annotation("Npgsql:IndexOperators", new[] { "gin_trgm_ops" });

            migrationBuilder.CreateIndex(
                name: "IX_Counterparties_Phone_trgm",
                table: "Counterparties",
                column: "Phone")
                .Annotation("Npgsql:IndexMethod", "gin")
                .Annotation("Npgsql:IndexOperators", new[] { "gin_trgm_ops" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Orders_CounterpartyId_CreatedAt_Id",
                table: "Orders");

            migrationBuilder.DropIndex(
                name: "IX_Orders_CreatedAt_Id",
                table: "Orders");

            migrationBuilder.DropIndex(
                name: "IX_Orders_OrderNumber_trgm",
                table: "Orders");

            migrationBuilder.DropIndex(
                name: "IX_OrderDrafts_Title_trgm",
                table: "OrderDrafts");

            migrationBuilder.DropIndex(
                name: "IX_Counterparties_Id_trgm",
                table: "Counterparties");

            migrationBuilder.DropIndex(
                name: "IX_Counterparties_Name_trgm",
                table: "Counterparties");

            migrationBuilder.DropIndex(
                name: "IX_Counterparties_Phone_trgm",
                table: "Counterparties");

            migrationBuilder.AlterDatabase()
                .OldAnnotation("Npgsql:PostgresExtension:pg_trgm", ",,");

            migrationBuilder.CreateIndex(
                name: "IX_Orders_CounterpartyId",
                table: "Orders",
                column: "CounterpartyId");
        }
    }
}
