namespace Sales.Application.Abstractions.Persistence;

public interface IReadSalesDbContext
{
    DbSet<Counterparty> Counterparties { get; }
    DbSet<CounterpartyCategoryPriceType> CounterpartyCategoryPriceTypes { get; }
}
