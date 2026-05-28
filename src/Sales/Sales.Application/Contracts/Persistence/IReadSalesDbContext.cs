namespace Sales.Application.Contracts.Persistence;

public interface IReadSalesDbContext
{
    DbSet<Counterparty> Counterparties { get; }
    DbSet<CounterpartyCategoryPriceType> CounterpartyCategoryPriceTypes { get; }
}
