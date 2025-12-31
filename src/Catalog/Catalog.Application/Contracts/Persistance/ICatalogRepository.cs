namespace Catalog.Application.Persistance;

public interface ICatalogRepository<T> : IRepositoryBase<T> where T : class, IAggregateRoot { }
