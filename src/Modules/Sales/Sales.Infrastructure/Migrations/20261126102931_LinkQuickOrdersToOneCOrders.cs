using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Sales.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class LinkQuickOrdersToOneCOrders : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<long>(
                name: "OneCOrderId",
                schema: "sales",
                table: "QuickOrders",
                type: "bigint",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "OneCOrderMessageId",
                schema: "sales",
                table: "QuickOrders",
                type: "uuid",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_QuickOrders_OneCOrderId",
                schema: "sales",
                table: "QuickOrders",
                column: "OneCOrderId");

            migrationBuilder.AddForeignKey(
                name: "FK_QuickOrders_Orders_OneCOrderId",
                schema: "sales",
                table: "QuickOrders",
                column: "OneCOrderId",
                principalSchema: "sales",
                principalTable: "Orders",
                principalColumn: "Id",
                onDelete: ReferentialAction.SetNull);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_QuickOrders_Orders_OneCOrderId",
                schema: "sales",
                table: "QuickOrders");

            migrationBuilder.DropIndex(
                name: "IX_QuickOrders_OneCOrderId",
                schema: "sales",
                table: "QuickOrders");

            migrationBuilder.DropColumn(
                name: "OneCOrderId",
                schema: "sales",
                table: "QuickOrders");

            migrationBuilder.DropColumn(
                name: "OneCOrderMessageId",
                schema: "sales",
                table: "QuickOrders");
        }
    }
}
