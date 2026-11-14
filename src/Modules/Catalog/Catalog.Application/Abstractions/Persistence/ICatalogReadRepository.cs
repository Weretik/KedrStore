namespace Catalog.Application.Abstractions.Persistence;

public interface ICatalogReadRepository<T> : IReadRepositoryBase<T> where T : class, IAggregateRoot { }
