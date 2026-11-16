using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata;
using Sales.Domain.Customers;
using Sales.Domain.Orders;
using Sales.Infrastructure.Persistence;
namespace IntegrationTests.Sales.Persistence.Orders;

public sealed class SalesOrderPersistenceTests
{
    [Fact]
    public void SalesModel_EnforcesOrderDeliveryAndCounterpartyRelationships()
    {
        using var dbContext = CreateDbContext();
        var order = RequireEntity<Order>(dbContext);
        var line = RequireEntity<OrderLine>(dbContext);

        Assert.Contains(order.GetIndexes(), index =>
            index.Properties.Select(property => property.Name).SequenceEqual([nameof(Order.OrderNumber)]) &&
            index.IsUnique);

        Assert.Contains(order.GetIndexes(), index =>
            index.Properties.Select(property => property.Name).SequenceEqual([nameof(Order.OneCMessageId)]) &&
            index.IsUnique);

        var lineForeignKey = line.GetForeignKeys().Single(foreignKey =>
            foreignKey.PrincipalEntityType.ClrType == typeof(Order));
        Assert.All(lineForeignKey.Properties, property => Assert.False(property.IsNullable));
        Assert.Equal(DeleteBehavior.Cascade, lineForeignKey.DeleteBehavior);

        var counterpartyForeignKey = order.GetForeignKeys().Single(foreignKey =>
            foreignKey.PrincipalEntityType.ClrType == typeof(Counterparty));
        Assert.Equal(DeleteBehavior.Restrict, counterpartyForeignKey.DeleteBehavior);

        var documentNumber = order.FindProperty(nameof(Order.OneCDocumentNumber));
        Assert.NotNull(documentNumber);
        Assert.True(documentNumber!.IsNullable);
        Assert.Equal(Order.OneCDocumentNumberMaxLength, documentNumber.GetMaxLength());

        var rejection = order.FindProperty(nameof(Order.OneCRejection));
        Assert.NotNull(rejection);
        Assert.Equal(Order.OneCRejectionMaxLength, rejection!.GetMaxLength());
    }

    [Fact]
    public void SalesModel_KeepsOrderMigrations()
    {
        using var dbContext = CreateDbContext();

        Assert.Contains(
            dbContext.Database.GetMigrations(),
            migration => migration.EndsWith("_AddSalesOrdersAndOneCOrderSync", StringComparison.Ordinal));
        Assert.Contains(
            dbContext.Database.GetMigrations(),
            migration => migration.EndsWith("_PersistOneCDocumentNumber", StringComparison.Ordinal));
        Assert.Contains(
            dbContext.Database.GetMigrations(),
            migration => migration.EndsWith("_AddOrderSyncRetryAudit", StringComparison.Ordinal));
        Assert.Contains(
            dbContext.Database.GetMigrations(),
            migration => migration.EndsWith("_MoveOneCOrdersToOutbox", StringComparison.Ordinal));
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
