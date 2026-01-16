using Catalog.Application.Persistence;
using Catalog.Infrastructure.DataBase;

namespace Catalog.Infrastructure.Repositories;

internal sealed class CatalogEfRepository<T>(CatalogDbContext dbContext)
    : RepositoryBase<T>(dbContext), ICatalogRepository<T>
    where T : class, IAggregateRoot { }
