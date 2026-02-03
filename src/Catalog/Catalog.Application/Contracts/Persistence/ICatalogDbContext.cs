namespace Catalog.Application.Contracts.Persistence;

public interface IReadCatalogDbContext
{
    DbSet<Product> Products { get; }
    DbSet<ProductPrice> ProductPrices { get; }
    DbSet<ProductCategory> Categories { get; }
    DbSet<PriceType> PriceTypes { get; }
}
