using Application.Catalog.Shared;

namespace Infrastructure.Catalog.Persistence;

internal sealed class CatalogEfRepository<T>(CatalogDbContext dbContext)
    : RepositoryBase<T>(dbContext), ICatalogRepository<T>
    where T : class, IAggregateRoot { }
