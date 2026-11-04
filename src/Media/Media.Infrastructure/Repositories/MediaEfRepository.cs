namespace Media.Infrastructure.Repositories;

internal sealed class MediaEfRepository<T>(MediaDbContext dbContext)
    : RepositoryBase<T>(dbContext), IMediaRepository<T>
    where T : class, IAggregateRoot { }
