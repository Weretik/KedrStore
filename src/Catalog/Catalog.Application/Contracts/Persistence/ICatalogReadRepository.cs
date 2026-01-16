namespace Catalog.Application.Persistence;

public interface ICatalogReadRepository<T> : IReadRepositoryBase<T> where T : class, IAggregateRoot { }
