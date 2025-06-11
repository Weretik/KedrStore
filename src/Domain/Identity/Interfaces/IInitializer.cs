namespace Domain.Identity.Interfaces;

public interface IInitializer
{
    Task InitializeAsync(IServiceProvider services, CancellationToken cancellationToken = default);
}
