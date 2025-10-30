namespace Application.Catalog.Shared;

public interface ICatalogReadRepository<T> : IReadRepositoryBase<T> where T : class, IAggregateRoot { }
