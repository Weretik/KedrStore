namespace Infrastructure.Catalog.Persistence;

internal sealed class CatalogEfRepository<T>(CatalogDbContext dbContext)
    : RepositoryBase<T>(dbContext), ICatalogRepository<T>, ICatalogReadRepository<T>
    where T : class, IAggregateRoot;
