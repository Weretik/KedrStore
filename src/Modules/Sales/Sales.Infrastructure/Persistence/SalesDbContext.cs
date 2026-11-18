using BuildingBlocks.Infrastructure.Persistence;
using Sales.Infrastructure.Persistence.Customers;
using BuildingBlocks.Infrastructure.Outbox.Persistence;
using Sales.Infrastructure.Persistence.Orders.SyncRetries;
using Sales.Infrastructure.Persistence.Orders.Idempotency;

namespace Sales.Infrastructure.Persistence;

public sealed class SalesDbContext(DbContextOptions<SalesDbContext> options)
    : DbContext(options), IReadSalesDbContext
{
    /// <summary>
    /// The module's own schema. Sales tables are still in <c>public</c>; new tables, starting with the outbox, go here
    /// (platform spec 006, R-001).
    /// </summary>
    public const string Schema = "sales";

    public DbSet<Counterparty> Counterparties => Set<Counterparty>();
    public DbSet<CounterpartyCategoryPriceType> CounterpartyCategoryPriceTypes =>
        Set<CounterpartyCategoryPriceType>();
    public DbSet<Order> Orders => Set<Order>();
    public DbSet<OrderLine> OrderLines => Set<OrderLine>();
    public DbSet<OrderDraft> OrderDrafts => Set<OrderDraft>();
    public DbSet<OrderDraftLine> OrderDraftLines => Set<OrderDraftLine>();
    internal DbSet<OrderIdempotencyRecord> OrderIdempotencyRecords => Set<OrderIdempotencyRecord>();
    internal DbSet<OrderSyncRetryAudit> OrderSyncRetryAudits => Set<OrderSyncRetryAudit>();
    internal DbSet<OrderDraftIdempotencyRecord> OrderDraftIdempotencyRecords => Set<OrderDraftIdempotencyRecord>();

    protected override void ConfigureConventions(ModelConfigurationBuilder configurationBuilder)
    {
        configurationBuilder.AddKernelIds();
        configurationBuilder.Properties<string>().HaveMaxLength(255);
        configurationBuilder.Properties<CounterpartyId>().HaveConversion<CounterpartyIdConverter>().HaveMaxLength(255);
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);
        modelBuilder.HasPostgresExtension("pg_trgm");

        modelBuilder.ApplyConfigurationsFromAssembly(
            typeof(SalesDbContext).Assembly,
            type => type.Namespace?.StartsWith("Sales.Infrastructure.Persistence", StringComparison.Ordinal) ?? false);
        modelBuilder.ApplyOutbox(Schema);
    }
}
