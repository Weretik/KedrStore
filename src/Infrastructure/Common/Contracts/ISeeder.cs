namespace Infrastructure.Common.Contracts;

public interface ISeeder
{
    Task SeedAsync(IServiceProvider services, CancellationToken cancellationToken = default);
}

