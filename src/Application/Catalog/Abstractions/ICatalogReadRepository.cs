namespace Application.Catalog.Abstractions;

public interface ICatalogReadRepository<T> : IReadRepositoryBase<T>
    where T : class, IAggregateRoot
{

}
