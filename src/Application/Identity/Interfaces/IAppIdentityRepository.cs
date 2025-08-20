namespace Application.Identity.Interfaces;

public interface IAppIdentityRepository<T> : IReadRepositoryBase<T>
    where T : class, IAggregateRoot
{

}
