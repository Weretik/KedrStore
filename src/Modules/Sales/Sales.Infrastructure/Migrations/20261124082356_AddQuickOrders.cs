using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace Sales.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddQuickOrders : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "QuickOrders",
                schema: "sales",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    FirstName = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: false),
                    Phone = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    PlacedAtUtc = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    Total = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                    TelegramSentAtUtc = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    TelegramLastAttemptAtUtc = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    TelegramLastError = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: true),
                    TelegramFailedAtUtc = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    xmin = table.Column<uint>(type: "xid", rowVersion: true, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_QuickOrders", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "QuickOrderItems",
                schema: "sales",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    ProductId = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: false),
                    Title = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: false),
                    UnitPrice = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                    Quantity = table.Column<int>(type: "integer", nullable: false),
                    QuickOrderId = table.Column<Guid>(type: "uuid", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_QuickOrderItems", x => x.Id);
                    table.ForeignKey(
                        name: "FK_QuickOrderItems_QuickOrders_QuickOrderId",
                        column: x => x.QuickOrderId,
                        principalSchema: "sales",
                        principalTable: "QuickOrders",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_QuickOrderItems_QuickOrderId",
                schema: "sales",
                table: "QuickOrderItems",
                column: "QuickOrderId");

            migrationBuilder.CreateIndex(
                name: "IX_QuickOrders_FirstName_trgm",
                schema: "sales",
                table: "QuickOrders",
                column: "FirstName")
                .Annotation("Npgsql:IndexMethod", "gin")
                .Annotation("Npgsql:IndexOperators", new[] { "gin_trgm_ops" });

            migrationBuilder.CreateIndex(
                name: "IX_QuickOrders_Phone_trgm",
                schema: "sales",
                table: "QuickOrders",
                column: "Phone")
                .Annotation("Npgsql:IndexMethod", "gin")
                .Annotation("Npgsql:IndexOperators", new[] { "gin_trgm_ops" });

            migrationBuilder.CreateIndex(
                name: "IX_QuickOrders_PlacedAtUtc_Id",
                schema: "sales",
                table: "QuickOrders",
                columns: new[] { "PlacedAtUtc", "Id" });

            migrationBuilder.CreateIndex(
                name: "IX_QuickOrders_Problem",
                schema: "sales",
                table: "QuickOrders",
                columns: new[] { "PlacedAtUtc", "Id" },
                filter: "\"TelegramSentAtUtc\" IS NULL AND (\"TelegramLastError\" IS NOT NULL OR \"TelegramFailedAtUtc\" IS NOT NULL)");

            // Spec sales/009, R-010: copy the site orders that so far lived only in the outbox (kept 30 days), with how
            // their Telegram delivery went. ON CONFLICT and NOT EXISTS make the copy safe to run again.
            migrationBuilder.Sql("""
                INSERT INTO sales."QuickOrders"
                    ("Id", "FirstName", "Phone", "PlacedAtUtc", "Total",
                     "TelegramSentAtUtc", "TelegramLastAttemptAtUtc", "TelegramLastError", "TelegramFailedAtUtc")
                SELECT m."Id",
                       left(m."Payload"->>'firstName', 30),
                       left(m."Payload"->>'phone', 20),
                       (m."Payload"->>'placedAtUtc')::timestamptz,
                       COALESCE((SELECT round(sum((l->>'unitPrice')::numeric * (l->>'quantity')::int), 2)
                                 FROM jsonb_array_elements(m."Payload"->'lines') AS l), 0),
                       m."ProcessedAtUtc",
                       m."LastAttemptAtUtc",
                       CASE WHEN m."ProcessedAtUtc" IS NULL THEN left(m."LastError", 1000) END,
                       CASE WHEN m."ProcessedAtUtc" IS NULL THEN m."FailedAtUtc" END
                FROM sales."OutboxMessages" AS m
                WHERE m."Type" = 'sales.quick-order-placed'
                ON CONFLICT ("Id") DO NOTHING;

                INSERT INTO sales."QuickOrderItems" ("QuickOrderId", "ProductId", "Title", "UnitPrice", "Quantity")
                SELECT m."Id",
                       left(l.value->>'productId', 64),
                       left(l.value->>'title', 500),
                       (l.value->>'unitPrice')::numeric,
                       (l.value->>'quantity')::int
                FROM sales."OutboxMessages" AS m
                CROSS JOIN LATERAL jsonb_array_elements(m."Payload"->'lines') WITH ORDINALITY AS l(value, position)
                WHERE m."Type" = 'sales.quick-order-placed'
                  AND NOT EXISTS (SELECT 1 FROM sales."QuickOrderItems" AS i WHERE i."QuickOrderId" = m."Id")
                ORDER BY m."Id", l.position;
                """);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "QuickOrderItems",
                schema: "sales");

            migrationBuilder.DropTable(
                name: "QuickOrders",
                schema: "sales");
        }
    }
}
