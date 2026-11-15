using BuildingBlocks.Infrastructure.Persistence;
using Catalog.Application.Abstractions.Persistence;

namespace Catalog.Infrastructure.Persistence;

internal sealed class CatalogReadEfRepository<T>(CatalogDbContext dbContext)
    : UnitOfWorkRepositoryBase<T>(dbContext), ICatalogReadRepository<T>
    where T : class, IAggregateRoot { }
