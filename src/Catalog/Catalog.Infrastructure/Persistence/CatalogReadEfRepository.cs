using Catalog.Application.Abstractions.Persistence;
using Catalog.Infrastructure.Persistence;

namespace Catalog.Infrastructure.Persistence;

internal sealed class CatalogReadEfRepository<T>(CatalogDbContext dbContext)
    : RepositoryBase<T>(dbContext), ICatalogReadRepository<T>
    where T : class, IAggregateRoot { }
