using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace Sales.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class MoveOneCOrdersToOutbox : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // Platform spec 006, stage 2: the order fields and the outbox column come first, then every row of the two
            // old queues moves to the shared outbox, and only then the old tables go.
            migrationBuilder.AddColumn<DateTimeOffset>(
                name: "LastAttemptAtUtc",
                schema: "sales",
                table: "OutboxMessages",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<DateTimeOffset>(
                name: "OneCAcceptedAtUtc",
                table: "Orders",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "OneCDocumentNumber",
                table: "Orders",
                type: "character varying(128)",
                maxLength: 128,
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "OneCMessageId",
                table: "Orders",
                type: "uuid",
                nullable: true);

            migrationBuilder.AddColumn<DateTimeOffset>(
                name: "OneCRejectedAtUtc",
                table: "Orders",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "OneCRejection",
                table: "Orders",
                type: "character varying(1000)",
                maxLength: 1000,
                nullable: true);

            migrationBuilder.Sql("""
                UPDATE "Orders" SET "OneCMessageId" = gen_random_uuid();

                UPDATE "Orders" AS o
                SET "OneCDocumentNumber" = s."OneCDocumentNumber",
                    "OneCAcceptedAtUtc" = COALESCE(s."AcceptedAtUtc", s."UpdatedAt", s."CreatedAt")
                FROM "OneCOrderSyncs" AS s
                WHERE s."OrderId" = o."Id" AND s."Status" = 'Accepted' AND s."OneCDocumentNumber" IS NOT NULL;

                UPDATE "Orders" AS o
                SET "OneCRejection" = COALESCE(NULLIF(btrim(s."LastErrorMessage"), ''), '1С відхилила замовлення без пояснення.'),
                    "OneCRejectedAtUtc" = COALESCE(s."UpdatedAt", s."CreatedAt")
                FROM "OneCOrderSyncs" AS s
                WHERE s."OrderId" = o."Id" AND s."Status" = 'BusinessError';

                -- Waiting, in-flight and retrying rows stay due with their attempts (one attempt is always left);
                -- dead-letter rows become failed messages.
                INSERT INTO sales."OutboxMessages"
                    ("Id", "Type", "Payload", "CreatedAtUtc", "Attempts", "NextAttemptAtUtc", "ProcessedAtUtc",
                     "FailedAtUtc", "LastError", "LastAttemptAtUtc")
                SELECT o."OneCMessageId", 'sales.order-placed', jsonb_build_object('orderId', o."Id"), s."CreatedAt",
                       CASE WHEN s."Status" = 'DeadLetter' THEN s."AttemptCount" ELSE LEAST(s."AttemptCount", 9) END,
                       CASE WHEN s."Status" = 'Sent' THEN now() ELSE s."NextAttemptAtUtc" END,
                       NULL,
                       CASE WHEN s."Status" = 'DeadLetter' THEN COALESCE(s."UpdatedAt", now()) END,
                       left(s."LastErrorMessage", 1000),
                       s."LastAttemptAtUtc"
                FROM "OneCOrderSyncs" AS s
                JOIN "Orders" AS o ON o."Id" = s."OrderId"
                WHERE s."Status" IN ('Pending', 'Sent', 'TransportError', 'RetryScheduled', 'DeadLetter')
                   OR (s."Status" = 'Accepted' AND s."OneCDocumentNumber" IS NULL);

                -- Dead letters whose Telegram file was not sent yet still get it.
                INSERT INTO sales."OutboxMessages"
                    ("Id", "Type", "Payload", "CreatedAtUtc", "Attempts", "NextAttemptAtUtc")
                SELECT gen_random_uuid(), 'sales.order-one-c-delivery-failed',
                       jsonb_build_object('orderId', o."Id", 'attempts', s."AttemptCount", 'lastError', s."LastErrorMessage",
                                          'failedAtUtc', COALESCE(s."UpdatedAt", now())),
                       now(), 0, now()
                FROM "OneCOrderSyncs" AS s
                JOIN "Orders" AS o ON o."Id" = s."OrderId"
                WHERE s."Status" = 'DeadLetter'
                  AND s."DeadLetterNotifiedAtUtc" IS NULL
                  AND s."DeadLetterNotificationExhaustedAtUtc" IS NULL;

                INSERT INTO sales."OutboxMessages"
                    ("Id", "Type", "Payload", "CreatedAtUtc", "Attempts", "NextAttemptAtUtc", "LastError")
                SELECT p."Id", 'sales.counterparty-removed',
                       jsonb_build_object('counterpartyId', p."CounterpartyId", 'identityUserId', p."IdentityUserId"),
                       p."CreatedAtUtc", 0, now(), p."LastError"
                FROM "PendingCounterpartyIdentityDeletions" AS p;
                """);

            migrationBuilder.AlterColumn<Guid>(
                name: "OneCMessageId",
                table: "Orders",
                type: "uuid",
                nullable: false,
                oldClrType: typeof(Guid),
                oldType: "uuid",
                oldNullable: true);

            migrationBuilder.DropForeignKey(
                name: "FK_OrderSyncRetryAudits_OneCOrderSyncs_OneCOrderSyncId",
                table: "OrderSyncRetryAudits");

            migrationBuilder.DropTable(
                name: "OneCOrderSyncs");

            migrationBuilder.DropTable(
                name: "PendingCounterpartyIdentityDeletions");

            migrationBuilder.DropIndex(
                name: "IX_OrderSyncRetryAudits_OneCOrderSyncId",
                table: "OrderSyncRetryAudits");

            migrationBuilder.DropColumn(
                name: "OneCOrderSyncId",
                table: "OrderSyncRetryAudits");

            migrationBuilder.CreateIndex(
                name: "IX_Orders_OneCMessageId",
                table: "Orders",
                column: "OneCMessageId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_OrderIdempotencyRecords_ExpiresAtUtc",
                table: "OrderIdempotencyRecords",
                column: "ExpiresAtUtc");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            // Down brings back the empty old tables only: the audit rows lost their sync ids, and outbox rows are not
            // moved back (restore the database backup when the old data matters).
            migrationBuilder.Sql("""DELETE FROM "OrderSyncRetryAudits";""");

            migrationBuilder.DropIndex(
                name: "IX_Orders_OneCMessageId",
                table: "Orders");

            migrationBuilder.DropIndex(
                name: "IX_OrderIdempotencyRecords_ExpiresAtUtc",
                table: "OrderIdempotencyRecords");

            migrationBuilder.DropColumn(
                name: "LastAttemptAtUtc",
                schema: "sales",
                table: "OutboxMessages");

            migrationBuilder.DropColumn(
                name: "OneCAcceptedAtUtc",
                table: "Orders");

            migrationBuilder.DropColumn(
                name: "OneCDocumentNumber",
                table: "Orders");

            migrationBuilder.DropColumn(
                name: "OneCMessageId",
                table: "Orders");

            migrationBuilder.DropColumn(
                name: "OneCRejectedAtUtc",
                table: "Orders");

            migrationBuilder.DropColumn(
                name: "OneCRejection",
                table: "Orders");

            migrationBuilder.AddColumn<long>(
                name: "OneCOrderSyncId",
                table: "OrderSyncRetryAudits",
                type: "bigint",
                nullable: false,
                defaultValue: 0L);

            migrationBuilder.CreateTable(
                name: "OneCOrderSyncs",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    AcceptedAtUtc = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    AttemptCount = table.Column<int>(type: "integer", nullable: false),
                    CreatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    DeadLetterNotificationAttemptCount = table.Column<int>(type: "integer", nullable: false),
                    DeadLetterNotificationExhaustedAtUtc = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    DeadLetterNotificationLastAttemptAtUtc = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    DeadLetterNotificationLastError = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: true),
                    DeadLetterNotificationNextAttemptAtUtc = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    DeadLetterNotifiedAtUtc = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    IsDeleted = table.Column<bool>(type: "boolean", nullable: false),
                    LastAttemptAtUtc = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    LastErrorCode = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: true),
                    LastErrorMessage = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: true),
                    LastHttpStatus = table.Column<int>(type: "integer", nullable: true),
                    NextAttemptAtUtc = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    OneCDocumentNumber = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: true),
                    OneCRequestPayloadHash = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: true),
                    OneCResponseBody = table.Column<string>(type: "character varying(2000)", maxLength: 2000, nullable: true),
                    OrderId = table.Column<long>(type: "bigint", nullable: false),
                    Status = table.Column<string>(type: "character varying(32)", maxLength: 32, nullable: false),
                    UpdatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    xmin = table.Column<uint>(type: "xid", rowVersion: true, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_OneCOrderSyncs", x => x.Id);
                    table.CheckConstraint("CK_OneCOrderSyncs_Status", "\"Status\" IN ('Pending', 'Sent', 'Accepted', 'BusinessError', 'TransportError', 'RetryScheduled', 'DeadLetter')");
                    table.ForeignKey(
                        name: "FK_OneCOrderSyncs_Orders_OrderId",
                        column: x => x.OrderId,
                        principalTable: "Orders",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "PendingCounterpartyIdentityDeletions",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    CounterpartyId = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: false),
                    CreatedAtUtc = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    IdentityUserId = table.Column<Guid>(type: "uuid", nullable: false),
                    LastAttemptAtUtc = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    LastError = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PendingCounterpartyIdentityDeletions", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_OrderSyncRetryAudits_OneCOrderSyncId",
                table: "OrderSyncRetryAudits",
                column: "OneCOrderSyncId");

            migrationBuilder.CreateIndex(
                name: "IX_OneCOrderSyncs_OrderId",
                table: "OneCOrderSyncs",
                column: "OrderId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_OneCOrderSyncs_Status_NextAttemptAtUtc",
                table: "OneCOrderSyncs",
                columns: new[] { "Status", "NextAttemptAtUtc" });

            migrationBuilder.CreateIndex(
                name: "IX_PendingCounterpartyIdentityDeletions_CounterpartyId_Identit~",
                table: "PendingCounterpartyIdentityDeletions",
                columns: new[] { "CounterpartyId", "IdentityUserId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_PendingCounterpartyIdentityDeletions_IdentityUserId",
                table: "PendingCounterpartyIdentityDeletions",
                column: "IdentityUserId");

            migrationBuilder.AddForeignKey(
                name: "FK_OrderSyncRetryAudits_OneCOrderSyncs_OneCOrderSyncId",
                table: "OrderSyncRetryAudits",
                column: "OneCOrderSyncId",
                principalTable: "OneCOrderSyncs",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }
    }
}
