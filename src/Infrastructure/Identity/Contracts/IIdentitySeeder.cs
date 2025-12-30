using BuildingBlocks.Infrastructure.Seeding;

namespace Infrastructure.Identity.Contracts;

public interface IIdentitySeeder : ISeeder
{
    new Task SeedAsync(IServiceProvider services, CancellationToken cancellationToken = default);
}
