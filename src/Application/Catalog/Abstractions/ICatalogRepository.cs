namespace Application.Catalog.Abstractions;

public interface ICatalogRepository<T> : IRepositoryBase<T>
    where T : class, IAggregateRoot
{
}
