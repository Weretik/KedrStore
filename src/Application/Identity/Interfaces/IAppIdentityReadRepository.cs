namespace Application.Identity.Interfaces;

public interface IAppIdentityReadRepository<T> : IReadRepositoryBase<T>
    where T : class, IAggregateRoot
{

}
