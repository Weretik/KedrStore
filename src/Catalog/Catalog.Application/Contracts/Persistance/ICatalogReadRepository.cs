namespace Catalog.Application.Persistance;

public interface ICatalogReadRepository<T> : IReadRepositoryBase<T> where T : class, IAggregateRoot { }
