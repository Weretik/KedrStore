namespace Infrastructure.Catalog.Persistence;

internal sealed class CatalogEfRepository<T>
    : RepositoryBase<T>, IReadRepositoryBase<T>, IRepositoryBase<T>
    where T : class, IAggregateRoot
{
    public CatalogEfRepository(CatalogDbContext dbContext) : base(dbContext) { }
}
