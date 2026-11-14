using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace Sales.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddOrderSyncRetryAudit : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "OrderSyncRetryAudits",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    OneCOrderSyncId = table.Column<long>(type: "bigint", nullable: false),
                    OrderId = table.Column<long>(type: "bigint", nullable: false),
                    OrderNumber = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: false),
                    PreviousStatus = table.Column<string>(type: "character varying(32)", maxLength: 32, nullable: false),
                    PreviousAttemptCount = table.Column<int>(type: "integer", nullable: false),
                    PreviousErrorCode = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: true),
                    PreviousErrorMessage = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: true),
                    Reason = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: false),
                    RequestedBy = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: false),
                    RequestedAtUtc = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_OrderSyncRetryAudits", x => x.Id);
                    table.ForeignKey(
                        name: "FK_OrderSyncRetryAudits_OneCOrderSyncs_OneCOrderSyncId",
                        column: x => x.OneCOrderSyncId,
                        principalTable: "OneCOrderSyncs",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_OrderSyncRetryAudits_OneCOrderSyncId",
                table: "OrderSyncRetryAudits",
                column: "OneCOrderSyncId");

            migrationBuilder.CreateIndex(
                name: "IX_OrderSyncRetryAudits_OrderId",
                table: "OrderSyncRetryAudits",
                column: "OrderId");

            migrationBuilder.CreateIndex(
                name: "IX_OrderSyncRetryAudits_RequestedAtUtc",
                table: "OrderSyncRetryAudits",
                column: "RequestedAtUtc");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "OrderSyncRetryAudits");
        }
    }
}
