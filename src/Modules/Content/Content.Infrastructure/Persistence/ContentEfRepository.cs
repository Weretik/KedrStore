using BuildingBlocks.Infrastructure.Persistence;

namespace Content.Infrastructure.Persistence;

internal sealed class ContentEfRepository<T>(ContentDbContext dbContext)
    : UnitOfWorkRepositoryBase<T>(dbContext), IContentRepository<T>
    where T : class, IAggregateRoot { }
