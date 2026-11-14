namespace Sales.Application.Abstractions.Persistence;

public interface ISalesRepository<T> : IRepositoryBase<T>
    where T : class, IAggregateRoot { }
