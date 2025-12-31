namespace Identity.Application;

public interface IAppIdentityRepository<T> : IReadRepositoryBase<T>
    where T : class, IAggregateRoot
{

}
