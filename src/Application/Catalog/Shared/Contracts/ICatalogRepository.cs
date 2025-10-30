namespace Application.Catalog.Shared;

public interface ICatalogRepository<T> : IRepositoryBase<T> where T : class, IAggregateRoot { }
