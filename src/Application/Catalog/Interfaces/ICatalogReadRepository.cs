namespace Application.Catalog.Interfaces;

public interface ICatalogReadRepository<T> : IReadRepositoryBase<T>
    where T : class, IAggregateRoot
{

}
