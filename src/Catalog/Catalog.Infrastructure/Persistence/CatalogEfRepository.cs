using Catalog.Application.Contracts.Persistence;
using Catalog.Infrastructure.Persistence;

namespace Catalog.Infrastructure.Persistence;

internal sealed class CatalogEfRepository<T>(CatalogDbContext dbContext)
    : RepositoryBase<T>(dbContext), ICatalogRepository<T>
    where T : class, IAggregateRoot { }
