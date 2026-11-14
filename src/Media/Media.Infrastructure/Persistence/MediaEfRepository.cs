namespace Media.Infrastructure.Persistence;

internal sealed class MediaEfRepository<T>(MediaDbContext dbContext)
    : RepositoryBase<T>(dbContext), IMediaRepository<T>
    where T : class, IAggregateRoot { }
