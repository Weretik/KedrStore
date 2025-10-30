namespace Infrastructure.Common.Contracts;

public interface IDatabaseMigrator
{
    Task MigrateAsync(IServiceProvider services, CancellationToken cancellationToken = default);
}
