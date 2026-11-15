using BuildingBlocks.Infrastructure.Persistence;

namespace Media.Infrastructure.Persistence;

internal sealed class MediaEfRepository<T>(MediaDbContext dbContext)
    : UnitOfWorkRepositoryBase<T>(dbContext), IMediaRepository<T>
    where T : class, IAggregateRoot { }
