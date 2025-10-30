namespace Application.Identity.Shared;

public interface IAppIdentityRepository<T> : IReadRepositoryBase<T>
    where T : class, IAggregateRoot
{

}
