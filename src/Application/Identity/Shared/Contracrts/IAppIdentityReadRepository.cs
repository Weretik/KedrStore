namespace Application.Identity.Shared;

public interface IAppIdentityReadRepository<T> : IReadRepositoryBase<T>
    where T : class, IAggregateRoot
{

}
