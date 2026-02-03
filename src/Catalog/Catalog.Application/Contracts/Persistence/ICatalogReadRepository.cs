namespace Catalog.Application.Contracts.Persistence;

public interface ICatalogReadRepository<T> : IReadRepositoryBase<T> where T : class, IAggregateRoot { }
