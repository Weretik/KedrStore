namespace Application.Catalog.Interfaces;

public interface ICatalogRepository<T> : IRepositoryBase<T> where T : class, IAggregateRoot { }
