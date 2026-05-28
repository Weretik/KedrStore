namespace Sales.Infrastructure.DataBase;

public sealed class SalesDbContext(DbContextOptions<SalesDbContext> options)
    : DbContext(options), IReadSalesDbContext
{
    public DbSet<Counterparty> Counterparties => Set<Counterparty>();
    public DbSet<CounterpartyCategoryPriceType> CounterpartyCategoryPriceTypes =>
        Set<CounterpartyCategoryPriceType>();

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
