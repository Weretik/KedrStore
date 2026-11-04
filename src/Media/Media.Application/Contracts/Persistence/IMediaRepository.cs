namespace Media.Application.Contracts.Persistence;

public interface IMediaRepository<T> : IRepositoryBase<T>
    where T : class, BuildingBlocks.Domain.Abstractions.IAggregateRoot { }
