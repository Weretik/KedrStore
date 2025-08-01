namespace Infrastructure.Common.Abstractions;

public interface IDatabaseMigrator
{
    Task MigrateAsync(IServiceProvider services, CancellationToken cancellationToken = default);
}
