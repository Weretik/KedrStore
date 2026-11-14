namespace Catalog.Application.Abstractions.Persistence;

public interface ICatalogRepository<T> : IRepositoryBase<T> where T : class, IAggregateRoot { }
