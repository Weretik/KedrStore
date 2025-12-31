namespace Identity.Infrastructure.Contracts;

public interface IIdentitySeeder : ISeeder
{
    new Task SeedAsync(IServiceProvider services, CancellationToken cancellationToken = default);
}
