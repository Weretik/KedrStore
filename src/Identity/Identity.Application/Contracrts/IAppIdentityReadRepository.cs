namespace Identity.Application;

public interface IAppIdentityReadRepository<T> : IReadRepositoryBase<T>
    where T : class, IAggregateRoot
{

}
