using IntegrationTests.TestSupport.Database;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql;
using Sales.Infrastructure.Persistence;

namespace IntegrationTests.Sales.OneC.OrderDelivery;

/// <summary>
/// Platform spec 006, SC-015: the migration moves the rows of the two old queues to the shared outbox without losing
/// attempts, errors or document numbers. Runs on its own database, migrated to the release before.
/// </summary>
[Collection(SalesPostgresCollection.Name)]
public sealed class OneCQueuesMigrationPostgresTests(SalesPostgresFixture postgres)
{
    private const string MigrationBefore = "20261115175825_AddSalesSearchIndexes";

    [SkippableFact]
    public async Task OldQueueRows_MoveToTheOutbox()
    {
        postgres.SkipIfUnavailable();
        var database = "kedr_sales_migration_" + Guid.NewGuid().ToString("N")[..8];
        await ExecuteAsync(postgres.ConnectionString, $"CREATE DATABASE {database}");
        var connectionString = new NpgsqlConnectionStringBuilder(postgres.ConnectionString) { Database = database }.ConnectionString;
        try
        {
            await using var db = new SalesDbContext(new DbContextOptionsBuilder<SalesDbContext>().UseNpgsql(connectionString).Options);
            var migrator = db.GetService<IMigrator>();
            await migrator.MigrateAsync(MigrationBefore);
            await db.Database.ExecuteSqlRawAsync(SeedOldRows);

            await migrator.MigrateAsync();

            var orders = await db.Database.SqlQueryRaw<OrderRow>("""
                SELECT "OrderNumber", "OneCDocumentNumber", "OneCRejection", "OneCMessageId" FROM sales."Orders" ORDER BY "Id"
                """).ToListAsync();
            Assert.Equal(["DOC-1", null, null, null, null], orders.Select(x => x.OneCDocumentNumber));
            Assert.Equal([null, "Немає ціни", null, null, null], orders.Select(x => x.OneCRejection));
            Assert.Equal(5, orders.Select(x => x.OneCMessageId).Distinct().Count());

            var messages = await db.Database.SqlQueryRaw<MessageRow>("""
                SELECT m."Type", m."Attempts", m."LastError", m."FailedAtUtc" IS NOT NULL AS "Failed",
                       o."OrderNumber" AS "OrderNumber"
                FROM sales."OutboxMessages" m LEFT JOIN sales."Orders" o ON o."OneCMessageId" = m."Id"
                ORDER BY m."Type", o."OrderNumber"
                """).ToListAsync();
            Assert.Equal(
            [
                new MessageRow("sales.counterparty-removed", 0, "temporary", false, null),
                new MessageRow("sales.order-one-c-delivery-failed", 0, null, false, null),
                new MessageRow("sales.order-placed", 0, null, false, "SO-3"),
                new MessageRow("sales.order-placed", 3, "timeout", false, "SO-4"),
                new MessageRow("sales.order-placed", 10, "gave up", true, "SO-5")
            ], messages);
            Assert.False(await TableExistsAsync(db, "OneCOrderSyncs"));
            Assert.False(await TableExistsAsync(db, "PendingCounterpartyIdentityDeletions"));
        }
        finally
        {
            NpgsqlConnection.ClearAllPools();
            await ExecuteAsync(postgres.ConnectionString, $"DROP DATABASE IF EXISTS {database} WITH (FORCE)");
        }
    }

    private const string SeedOldRows = """
        INSERT INTO "Counterparties" ("Id", "IdentityUserId", "Name", "Email", "DefaultPriceTypeId", "CreatedAt", "IsDeleted")
        VALUES ('cp-1', gen_random_uuid(), 'Клієнт', 'cp-1@example.com', 1, now(), false);
        INSERT INTO "Orders" ("OrderNumber", "CounterpartyId", "CreatedAt", "IsDeleted")
        SELECT 'SO-' || n, 'cp-1', now(), false FROM generate_series(1, 5) AS n;
        INSERT INTO "OneCOrderSyncs"
            ("OrderId", "Status", "AttemptCount", "NextAttemptAtUtc", "LastErrorMessage", "OneCDocumentNumber",
             "AcceptedAtUtc", "CreatedAt", "IsDeleted", "DeadLetterNotificationAttemptCount")
        SELECT o."Id",
               (ARRAY['Accepted', 'BusinessError', 'Pending', 'RetryScheduled', 'DeadLetter'])[n],
               (ARRAY[1, 1, 0, 3, 10])[n], now(),
               (ARRAY[NULL, 'Немає ціни', NULL, 'timeout', 'gave up'])[n],
               (ARRAY['DOC-1', NULL, NULL, NULL, NULL])[n],
               CASE WHEN n = 1 THEN now() END, now(), false, 0
        FROM "Orders" o JOIN generate_series(1, 5) AS n ON o."OrderNumber" = 'SO-' || n;
        INSERT INTO "PendingCounterpartyIdentityDeletions" ("Id", "CounterpartyId", "IdentityUserId", "CreatedAtUtc", "LastError")
        VALUES (gen_random_uuid(), 'gone-cp', gen_random_uuid(), now(), 'temporary');
        """;

    private static async Task ExecuteAsync(string connectionString, string sql)
    {
        await using var connection = new NpgsqlConnection(connectionString);
        await connection.OpenAsync();
        await using var command = new NpgsqlCommand(sql, connection);
        await command.ExecuteNonQueryAsync();
    }

    private static Task<bool> TableExistsAsync(SalesDbContext db, string table)
        => db.Database.SqlQueryRaw<bool>($"SELECT to_regclass('public.\"{table}\"') IS NOT NULL AS \"Value\"").SingleAsync();

    private sealed record OrderRow(string OrderNumber, string? OneCDocumentNumber, string? OneCRejection, Guid OneCMessageId);

    private sealed record MessageRow(string Type, int Attempts, string? LastError, bool Failed, string? OrderNumber);
}
