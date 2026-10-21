using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace Sales.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddAdminOrderDrafts : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "OrderDrafts",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    CounterpartyId = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: false),
                    Title = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    Note = table.Column<string>(type: "character varying(2000)", maxLength: 2000, nullable: true),
                    CreatedAtUtc = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    UpdatedAtUtc = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    Version = table.Column<string>(type: "character varying(32)", maxLength: 32, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_OrderDrafts", x => x.Id);
                    table.ForeignKey(
                        name: "FK_OrderDrafts_Counterparties_CounterpartyId",
                        column: x => x.CounterpartyId,
                        principalTable: "Counterparties",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "OrderDraftIdempotencyRecords",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    IdempotencyKey = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: false),
                    RequestHash = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: false),
                    DraftId = table.Column<long>(type: "bigint", nullable: false),
                    CreatedAtUtc = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_OrderDraftIdempotencyRecords", x => x.Id);
                    table.ForeignKey(
                        name: "FK_OrderDraftIdempotencyRecords_OrderDrafts_DraftId",
                        column: x => x.DraftId,
                        principalTable: "OrderDrafts",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "OrderDraftLines",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    ProductId = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: false),
                    Quantity = table.Column<int>(type: "integer", nullable: false),
                    DraftId = table.Column<long>(type: "bigint", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_OrderDraftLines", x => x.Id);
                    table.CheckConstraint("CK_OrderDraftLines_Quantity", "\"Quantity\" > 0");
                    table.ForeignKey(
                        name: "FK_OrderDraftLines_OrderDrafts_DraftId",
                        column: x => x.DraftId,
                        principalTable: "OrderDrafts",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_OrderDraftIdempotencyRecords_DraftId",
                table: "OrderDraftIdempotencyRecords",
                column: "DraftId");

            migrationBuilder.CreateIndex(
                name: "IX_OrderDraftIdempotencyRecords_IdempotencyKey",
                table: "OrderDraftIdempotencyRecords",
                column: "IdempotencyKey",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_OrderDraftLines_DraftId_ProductId",
                table: "OrderDraftLines",
                columns: new[] { "DraftId", "ProductId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_OrderDrafts_CounterpartyId_UpdatedAtUtc_Id",
                table: "OrderDrafts",
                columns: new[] { "CounterpartyId", "UpdatedAtUtc", "Id" });

            migrationBuilder.CreateIndex(
                name: "IX_OrderDrafts_UpdatedAtUtc_Id",
                table: "OrderDrafts",
                columns: new[] { "UpdatedAtUtc", "Id" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "OrderDraftIdempotencyRecords");

            migrationBuilder.DropTable(
                name: "OrderDraftLines");

            migrationBuilder.DropTable(
                name: "OrderDrafts");
        }
    }
}
