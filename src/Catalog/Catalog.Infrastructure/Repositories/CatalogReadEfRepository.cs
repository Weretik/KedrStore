using Catalog.Application.Persistance;
using Catalog.Infrastructure.DataBase;

namespace Catalog.Infrastructure.Repositories;

internal sealed class CatalogReadEfRepository<T>(CatalogDbContext dbContext)
    : RepositoryBase<T>(dbContext), ICatalogReadRepository<T>
    where T : class, IAggregateRoot { }
