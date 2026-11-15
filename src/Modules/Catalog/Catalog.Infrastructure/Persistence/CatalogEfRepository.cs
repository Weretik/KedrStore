using BuildingBlocks.Infrastructure.Persistence;
using Catalog.Application.Abstractions.Persistence;

namespace Catalog.Infrastructure.Persistence;

internal sealed class CatalogEfRepository<T>(CatalogDbContext dbContext)
    : UnitOfWorkRepositoryBase<T>(dbContext), ICatalogRepository<T>
    where T : class, IAggregateRoot { }
