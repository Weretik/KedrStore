namespace Content.Infrastructure.Repositories;

internal sealed class ContentEfRepository<T>(ContentDbContext dbContext)
    : RepositoryBase<T>(dbContext), IContentRepository<T>
    where T : class, IAggregateRoot { }
