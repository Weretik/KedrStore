namespace Sales.Infrastructure.DataBase;

public sealed class SalesDbContext(DbContextOptions<SalesDbContext> options)
    : DbContext(options), IReadSalesDbContext
{
    public DbSet<Counterparty> Counterparties => Set<Counterparty>();
    public DbSet<CounterpartyCategoryPriceType> CounterpartyCategoryPriceTypes =>
        Set<CounterpartyCategoryPriceType>();
    public DbSet<Order> Orders => Set<Order>();
    public DbSet<OrderLine> OrderLines => Set<OrderLine>();
    public DbSet<OneCOrderSync> OneCOrderSyncs => Set<OneCOrderSync>();
    internal DbSet<OrderIdempotencyRecord> OrderIdempotencyRecords => Set<OrderIdempotencyRecord>();
    internal DbSet<OrderSyncRetryAudit> OrderSyncRetryAudits => Set<OrderSyncRetryAudit>();

    protected override void ConfigureConventions(ModelConfigurationBuilder configurationBuilder)
    {
        configurationBuilder.Properties<string>().HaveMaxLength(255);
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.ApplyConfigurationsFromAssembly(
            typeof(SalesDbContext).Assembly,
            type => type.Namespace?.StartsWith("Sales.Infrastructure") ?? false);
    }
}
