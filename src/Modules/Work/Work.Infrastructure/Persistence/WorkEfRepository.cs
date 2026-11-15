using BuildingBlocks.Infrastructure.Persistence;

namespace Work.Infrastructure.Persistence;

internal sealed class WorkEfRepository<T>(WorkDbContext dbContext)
    : UnitOfWorkRepositoryBase<T>(dbContext), IWorkRepository<T>
    where T : class, IAggregateRoot { }
