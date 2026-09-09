using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Sales.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddDeadLetterTelegramNotificationRetry : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "DeadLetterNotificationAttemptCount",
                table: "OneCOrderSyncs",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<DateTimeOffset>(
                name: "DeadLetterNotificationExhaustedAtUtc",
                table: "OneCOrderSyncs",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<DateTimeOffset>(
                name: "DeadLetterNotificationLastAttemptAtUtc",
                table: "OneCOrderSyncs",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "DeadLetterNotificationLastError",
                table: "OneCOrderSyncs",
                type: "character varying(1000)",
                maxLength: 1000,
                nullable: true);

            migrationBuilder.AddColumn<DateTimeOffset>(
                name: "DeadLetterNotificationNextAttemptAtUtc",
                table: "OneCOrderSyncs",
                type: "timestamp with time zone",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "DeadLetterNotificationAttemptCount",
                table: "OneCOrderSyncs");

            migrationBuilder.DropColumn(
                name: "DeadLetterNotificationExhaustedAtUtc",
                table: "OneCOrderSyncs");

            migrationBuilder.DropColumn(
                name: "DeadLetterNotificationLastAttemptAtUtc",
                table: "OneCOrderSyncs");

            migrationBuilder.DropColumn(
                name: "DeadLetterNotificationLastError",
                table: "OneCOrderSyncs");

            migrationBuilder.DropColumn(
                name: "DeadLetterNotificationNextAttemptAtUtc",
                table: "OneCOrderSyncs");
        }
    }
}
