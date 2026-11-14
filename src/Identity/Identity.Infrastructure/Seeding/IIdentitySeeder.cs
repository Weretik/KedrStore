namespace Identity.Infrastructure.Seeding;

public interface IIdentitySeeder : ISeeder
{
    new Task SeedAsync(IServiceProvider services, CancellationToken cancellationToken = default);
}
