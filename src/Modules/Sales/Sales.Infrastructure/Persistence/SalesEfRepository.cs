using BuildingBlocks.Infrastructure.Persistence;

namespace Sales.Infrastructure.Persistence;

internal sealed class SalesEfRepository<T>(SalesDbContext dbContext)
    : UnitOfWorkRepositoryBase<T>(dbContext), ISalesRepository<T>
    where T : class, IAggregateRoot { }
