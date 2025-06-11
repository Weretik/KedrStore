namespace Domain.Abstractions;

public interface IInitializer
{
    Task InitializeAsync(IServiceProvider services, CancellationToken cancellationToken = default);
}
