namespace Catalog.Application.Contracts.Persistence;

public interface ICatalogRepository<T> : IRepositoryBase<T> where T : class, IAggregateRoot { }
