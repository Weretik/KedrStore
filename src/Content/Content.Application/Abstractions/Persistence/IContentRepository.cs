namespace Content.Application.Abstractions.Persistence;

public interface IContentRepository<T> : IRepositoryBase<T>
    where T : class, BuildingBlocks.Domain.Abstractions.IAggregateRoot { }
