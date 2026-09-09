using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata;
using Sales.Domain.Customers.Entities;
using Sales.Domain.Orders.Entities;
using Sales.Infrastructure.DataBase;

namespace IntegrationTests.Sales.Persistence;

public sealed class SalesOrderPersistenceTests
{
    [Fact]
    public void SalesModel_EnforcesOrderDeliveryAndCounterpartyRelationships()
    {
        using var dbContext = CreateDbContext();
        var order = RequireEntity<Order>(dbContext);
        var sync = RequireEntity<OneCOrderSync>(dbContext);
        var line = RequireEntity<OrderLine>(dbContext);

        Assert.Contains(order.GetIndexes(), index =>
            index.Properties.Select(property => property.Name).SequenceEqual([nameof(Order.OrderNumber)]) &&
            index.IsUnique);

        var syncForeignKey = sync.GetForeignKeys().Single(foreignKey =>
            foreignKey.PrincipalEntityType.ClrType == typeof(Order));
        Assert.True(syncForeignKey.IsUnique);
        Assert.Equal(DeleteBehavior.Cascade, syncForeignKey.DeleteBehavior);

        var lineForeignKey = line.GetForeignKeys().Single(foreignKey =>
            foreignKey.PrincipalEntityType.ClrType == typeof(Order));
        Assert.All(lineForeignKey.Properties, property => Assert.False(property.IsNullable));
        Assert.Equal(DeleteBehavior.Cascade, lineForeignKey.DeleteBehavior);

        var counterpartyForeignKey = order.GetForeignKeys().Single(foreignKey =>
            foreignKey.PrincipalEntityType.ClrType == typeof(Counterparty));
        Assert.Equal(DeleteBehavior.Restrict, counterpartyForeignKey.DeleteBehavior);

        var documentNumber = sync.FindProperty(nameof(OneCOrderSync.OneCDocumentNumber));
        Assert.NotNull(documentNumber);
        Assert.True(documentNumber!.IsNullable);
        Assert.Equal(128, documentNumber.GetMaxLength());

        var concurrencyToken = sync.FindProperty("xmin");
        Assert.NotNull(concurrencyToken);
        Assert.True(concurrencyToken!.IsConcurrencyToken);
        Assert.Equal(ValueGenerated.OnAddOrUpdate, concurrencyToken.ValueGenerated);
    }

    [Fact]
    public void SalesModel_MapsDueRecordIndexAndOrderMigration()
    {
        using var dbContext = CreateDbContext();
        var sync = RequireEntity<OneCOrderSync>(dbContext);

        Assert.Contains(sync.GetIndexes(), index => index.Properties.Select(property => property.Name)
            .SequenceEqual([nameof(OneCOrderSync.Status), nameof(OneCOrderSync.NextAttemptAtUtc)]));
        Assert.Contains(
            dbContext.Database.GetMigrations(),
            migration => migration.EndsWith("_AddSalesOrdersAndOneCOrderSync", StringComparison.Ordinal));
        Assert.Contains(
            dbContext.Database.GetMigrations(),
            migration => migration.EndsWith("_PersistOneCDocumentNumber", StringComparison.Ordinal));
        Assert.Contains(
            dbContext.Database.GetMigrations(),
            migration => migration.EndsWith("_AddOrderSyncRetryAudit", StringComparison.Ordinal));
    }

    private static SalesDbContext CreateDbContext()
    {
        var options = new DbContextOptionsBuilder<SalesDbContext>()
            .UseNpgsql("Host=localhost;Database=kedr_test;Username=kedr_user;Password=not-used")
            .Options;

        return new SalesDbContext(options);
    }

    private static IEntityType RequireEntity<TEntity>(SalesDbContext dbContext)
        => Assert.IsAssignableFrom<IEntityType>(dbContext.Model.FindEntityType(typeof(TEntity)));
}
