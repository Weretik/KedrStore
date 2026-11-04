namespace Content.Application.Contracts.Persistence;

public interface IContentRepository<T> : IRepositoryBase<T>
    where T : class, BuildingBlocks.Domain.Abstractions.IAggregateRoot { }
