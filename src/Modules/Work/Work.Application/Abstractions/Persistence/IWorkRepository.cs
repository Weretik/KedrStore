namespace Work.Application.Abstractions.Persistence;

public interface IWorkRepository<T> : IRepositoryBase<T>
    where T : class, BuildingBlocks.Domain.Abstractions.IAggregateRoot { }
