using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Sales.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddPendingCounterpartyIdentityDeletions : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "PendingCounterpartyIdentityDeletions",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    CounterpartyId = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: false),
                    IdentityUserId = table.Column<Guid>(type: "uuid", nullable: false),
                    CreatedAtUtc = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    LastAttemptAtUtc = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    LastError = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PendingCounterpartyIdentityDeletions", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_PendingCounterpartyIdentityDeletions_CounterpartyId_Identit~",
                table: "PendingCounterpartyIdentityDeletions",
                columns: new[] { "CounterpartyId", "IdentityUserId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_PendingCounterpartyIdentityDeletions_IdentityUserId",
                table: "PendingCounterpartyIdentityDeletions",
                column: "IdentityUserId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "PendingCounterpartyIdentityDeletions");
        }
    }
}
