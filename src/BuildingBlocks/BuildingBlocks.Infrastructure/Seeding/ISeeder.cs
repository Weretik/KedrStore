namespace BuildingBlocks.Infrastructure.Seeding;

public interface ISeeder
{
    Task SeedAsync(IServiceProvider services, CancellationToken cancellationToken = default);
}
