using IntegrationTests.TestSupport.Database;
using Microsoft.EntityFrameworkCore;
using Npgsql;
using Sales.Domain.Customers.Entities;
using Sales.Domain.Orders.Entities;
using Sales.Infrastructure.DataBase;

namespace IntegrationTests.Sales.Persistence;

[Collection(SalesPostgresCollection.Name)]
public sealed class OrderDraftPostgresTests(SalesPostgresFixture postgres)
{
    private const string CounterpartyId = "draft-cp";

    [SkippableFact]
    public async Task Create_PersistsDraftWithLines()
    {
        postgres.SkipIfUnavailable();
        await ResetDatabaseAsync();
        var draftId = await SeedDraftAsync([OrderDraftLine.Create("101", 2, -50.25m), OrderDraftLine.Create("102", 1)]);

        await using var db = postgres.CreateContext();
        var draft = await db.OrderDrafts.AsNoTracking().Include(x => x.Lines).SingleAsync(x => x.Id == draftId);

        Assert.Equal("Draft", draft.Title);
        Assert.Equal(CounterpartyId, draft.CounterpartyId);
        Assert.Equal(32, draft.Version.Length);
        Assert.Equal([("101", 2, -50.25m), ("102", 1, 0m)],
            draft.Lines.OrderBy(x => x.ProductId).Select(x => (x.ProductId, x.Quantity, x.PriceAdjustment)));
    }

    [SkippableFact]
    public async Task Update_ReplacesLines_AndRejectsStaleVersion()
    {
        postgres.SkipIfUnavailable();
        await ResetDatabaseAsync();
        var draftId = await SeedDraftAsync([OrderDraftLine.Create("101", 2), OrderDraftLine.Create("102", 1)]);

        await using var first = postgres.CreateContext();
        await using var second = postgres.CreateContext();
        var current = await first.OrderDrafts.Include(x => x.Lines).SingleAsync(x => x.Id == draftId);
        var stale = await second.OrderDrafts.Include(x => x.Lines).SingleAsync(x => x.Id == draftId);

        current.Update("Updated", "Note", CounterpartyId, [OrderDraftLine.Create("103", 5)], DateTimeOffset.UtcNow);
        await first.SaveChangesAsync();
        stale.Update("Stale", null, CounterpartyId, [OrderDraftLine.Create("104", 1)], DateTimeOffset.UtcNow);

        await Assert.ThrowsAsync<DbUpdateConcurrencyException>(() => second.SaveChangesAsync());
        await using var verification = postgres.CreateContext();
        var persisted = await verification.OrderDrafts.AsNoTracking().Include(x => x.Lines).SingleAsync();
        Assert.Equal("Updated", persisted.Title);
        Assert.Equal(current.Version, persisted.Version);
        Assert.Equal([("103", 5)], persisted.Lines.Select(x => (x.ProductId, x.Quantity)));
        Assert.Equal(1, await CountAsync(verification, "OrderDraftLines"));
    }

    [SkippableFact]
    public async Task Delete_CascadesLinesAndIdempotencyRecords()
    {
        postgres.SkipIfUnavailable();
        await ResetDatabaseAsync();
        var draftId = await SeedDraftAsync([OrderDraftLine.Create("101", 2)]);
        await InsertIdempotencyRecordAsync(Guid.NewGuid().ToString("D"), draftId.Value);

        await using (var db = postgres.CreateContext())
        {
            db.OrderDrafts.Remove(await db.OrderDrafts.SingleAsync(x => x.Id == draftId));
            await db.SaveChangesAsync();
        }

        await using var verification = postgres.CreateContext();
        Assert.Equal(0, await CountAsync(verification, "OrderDrafts"));
        Assert.Equal(0, await CountAsync(verification, "OrderDraftLines"));
        Assert.Equal(0, await CountAsync(verification, "OrderDraftIdempotencyRecords"));
    }

    [SkippableFact]
    public async Task IdempotencyKey_IsUniqueAcrossDrafts()
    {
        postgres.SkipIfUnavailable();
        await ResetDatabaseAsync();
        var key = Guid.NewGuid().ToString("D");
        var firstDraft = await SeedDraftAsync([OrderDraftLine.Create("101", 1)]);
        var secondDraft = await SeedDraftAsync([OrderDraftLine.Create("102", 1)]);
        await InsertIdempotencyRecordAsync(key, firstDraft.Value);

        var error = await Assert.ThrowsAsync<PostgresException>(
            () => InsertIdempotencyRecordAsync(key, secondDraft.Value));

        Assert.Equal(PostgresErrorCodes.UniqueViolation, error.SqlState);
    }

    [SkippableFact]
    public async Task Create_RejectsUnknownCustomerAndDuplicateProduct()
    {
        postgres.SkipIfUnavailable();
        await ResetDatabaseAsync();
        var draftId = await SeedDraftAsync([OrderDraftLine.Create("101", 1)]);

        await using (var db = postgres.CreateContext())
        {
            db.OrderDrafts.Add(OrderDraft.Create("Orphan", null, "missing-cp",
                [OrderDraftLine.Create("101", 1)], DateTimeOffset.UtcNow));
            var orphan = await Assert.ThrowsAsync<DbUpdateException>(() => db.SaveChangesAsync());
            Assert.Equal(PostgresErrorCodes.ForeignKeyViolation,
                Assert.IsType<PostgresException>(orphan.InnerException).SqlState);
        }

        await using var connection = new NpgsqlConnection(postgres.ConnectionString);
        await connection.OpenAsync();
        await using var duplicate = new NpgsqlCommand(
            "INSERT INTO \"OrderDraftLines\" (\"DraftId\", \"ProductId\", \"Quantity\") VALUES (@draft, '101', 1)",
            connection);
        duplicate.Parameters.AddWithValue("draft", draftId.Value);
        var error = await Assert.ThrowsAsync<PostgresException>(() => duplicate.ExecuteNonQueryAsync());
        Assert.Equal(PostgresErrorCodes.UniqueViolation, error.SqlState);
    }

    private async Task ResetDatabaseAsync()
    {
        await using var db = postgres.CreateContext();
        await db.OrderDrafts.ExecuteDeleteAsync();
        await db.Database.ExecuteSqlRawAsync("DELETE FROM \"PendingCounterpartyIdentityDeletions\"");
        await db.Database.ExecuteSqlRawAsync("DELETE FROM \"OrderSyncRetryAudits\"");
        await db.Database.ExecuteSqlRawAsync("DELETE FROM \"OrderIdempotencyRecords\"");
        await db.OneCOrderSyncs.ExecuteDeleteAsync();
        await db.Orders.ExecuteDeleteAsync();
        await db.CounterpartyCategoryPriceTypes.ExecuteDeleteAsync();
        await db.Counterparties.IgnoreQueryFilters().ExecuteDeleteAsync();
        db.Counterparties.Add(Counterparty.Create(CounterpartyId, Guid.NewGuid(), "Draft customer",
            "draft@example.com", null, 1, DateTimeOffset.UtcNow));
        await db.SaveChangesAsync();
    }

    private async Task<global::Sales.Domain.Orders.ValueObjects.OrderDraftId> SeedDraftAsync(
        IEnumerable<OrderDraftLine> lines)
    {
        await using var db = postgres.CreateContext();
        var draft = OrderDraft.Create("Draft", null, CounterpartyId, lines, DateTimeOffset.UtcNow);
        db.OrderDrafts.Add(draft);
        await db.SaveChangesAsync();
        return draft.Id;
    }

    private async Task InsertIdempotencyRecordAsync(string key, long draftId)
    {
        await using var connection = new NpgsqlConnection(postgres.ConnectionString);
        await connection.OpenAsync();
        await using var command = new NpgsqlCommand(
            "INSERT INTO \"OrderDraftIdempotencyRecords\" (\"IdempotencyKey\", \"RequestHash\", \"DraftId\", \"CreatedAtUtc\") " +
            "VALUES (@key, 'hash', @draft, now())",
            connection);
        command.Parameters.AddWithValue("key", key);
        command.Parameters.AddWithValue("draft", draftId);
        await command.ExecuteNonQueryAsync();
    }

    private static Task<int> CountAsync(SalesDbContext db, string table)
        => db.Database.SqlQueryRaw<int>(table switch
        {
            "OrderDrafts" => "SELECT COUNT(*)::integer AS \"Value\" FROM \"OrderDrafts\"",
            "OrderDraftLines" => "SELECT COUNT(*)::integer AS \"Value\" FROM \"OrderDraftLines\"",
            "OrderDraftIdempotencyRecords" => "SELECT COUNT(*)::integer AS \"Value\" FROM \"OrderDraftIdempotencyRecords\"",
            _ => throw new ArgumentOutOfRangeException(nameof(table))
        }).SingleAsync();
}
