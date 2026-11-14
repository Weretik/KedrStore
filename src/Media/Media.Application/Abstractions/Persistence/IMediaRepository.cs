namespace Media.Application.Abstractions.Persistence;

public interface IMediaRepository<T> : IRepositoryBase<T>
    where T : class, BuildingBlocks.Domain.Abstractions.IAggregateRoot { }
