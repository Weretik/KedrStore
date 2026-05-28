namespace Sales.Infrastructure.Repositories;

internal sealed class SalesEfRepository<T>(SalesDbContext dbContext)
    : RepositoryBase<T>(dbContext), ISalesRepository<T>
    where T : class, IAggregateRoot { }
