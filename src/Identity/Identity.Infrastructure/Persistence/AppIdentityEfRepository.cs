using Identity.Application;

namespace Identity.Infrastructure.Persistence;

public class AppIdentityEfRepository<T>
    : RepositoryBase<T>, IAppIdentityReadRepository<T>, IAppIdentityRepository<T>
    where T : class, IAggregateRoot
{
    public AppIdentityEfRepository(IdentityDbContext dbContext)
        : base(dbContext)
    {
    }
}
