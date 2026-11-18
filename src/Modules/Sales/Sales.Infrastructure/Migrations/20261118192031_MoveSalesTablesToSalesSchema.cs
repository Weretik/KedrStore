using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Sales.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class MoveSalesTablesToSalesSchema : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameTable(
                name: "OrderSyncRetryAudits",
                newName: "OrderSyncRetryAudits",
                newSchema: "sales");

            migrationBuilder.RenameTable(
                name: "Orders",
                newName: "Orders",
                newSchema: "sales");

            migrationBuilder.RenameTable(
                name: "OrderLines",
                newName: "OrderLines",
                newSchema: "sales");

            migrationBuilder.RenameTable(
                name: "OrderIdempotencyRecords",
                newName: "OrderIdempotencyRecords",
                newSchema: "sales");

            migrationBuilder.RenameTable(
                name: "OrderDrafts",
                newName: "OrderDrafts",
                newSchema: "sales");

            migrationBuilder.RenameTable(
                name: "OrderDraftLines",
                newName: "OrderDraftLines",
                newSchema: "sales");

            migrationBuilder.RenameTable(
                name: "OrderDraftIdempotencyRecords",
                newName: "OrderDraftIdempotencyRecords",
                newSchema: "sales");

            migrationBuilder.RenameTable(
                name: "CounterpartyCategoryPriceTypes",
                newName: "CounterpartyCategoryPriceTypes",
                newSchema: "sales");

            migrationBuilder.RenameTable(
                name: "Counterparties",
                newName: "Counterparties",
                newSchema: "sales");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameTable(
                name: "OrderSyncRetryAudits",
                schema: "sales",
                newName: "OrderSyncRetryAudits");

            migrationBuilder.RenameTable(
                name: "Orders",
                schema: "sales",
                newName: "Orders");

            migrationBuilder.RenameTable(
                name: "OrderLines",
                schema: "sales",
                newName: "OrderLines");

            migrationBuilder.RenameTable(
                name: "OrderIdempotencyRecords",
                schema: "sales",
                newName: "OrderIdempotencyRecords");

            migrationBuilder.RenameTable(
                name: "OrderDrafts",
                schema: "sales",
                newName: "OrderDrafts");

            migrationBuilder.RenameTable(
                name: "OrderDraftLines",
                schema: "sales",
                newName: "OrderDraftLines");

            migrationBuilder.RenameTable(
                name: "OrderDraftIdempotencyRecords",
                schema: "sales",
                newName: "OrderDraftIdempotencyRecords");

            migrationBuilder.RenameTable(
                name: "CounterpartyCategoryPriceTypes",
                schema: "sales",
                newName: "CounterpartyCategoryPriceTypes");

            migrationBuilder.RenameTable(
                name: "Counterparties",
                schema: "sales",
                newName: "Counterparties");
        }
    }
}
