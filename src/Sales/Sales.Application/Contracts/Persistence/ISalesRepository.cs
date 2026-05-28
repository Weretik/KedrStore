namespace Sales.Application.Contracts.Persistence;

public interface ISalesRepository<T> : IRepositoryBase<T>
    where T : class, IAggregateRoot { }
