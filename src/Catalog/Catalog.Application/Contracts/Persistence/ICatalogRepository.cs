namespace Catalog.Application.Persistence;

public interface ICatalogRepository<T> : IRepositoryBase<T> where T : class, IAggregateRoot { }
