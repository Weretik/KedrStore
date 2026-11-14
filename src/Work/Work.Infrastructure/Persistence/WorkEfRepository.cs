namespace Work.Infrastructure.Persistence;

internal sealed class WorkEfRepository<T>(WorkDbContext dbContext)
    : RepositoryBase<T>(dbContext), IWorkRepository<T>
    where T : class, IAggregateRoot { }
