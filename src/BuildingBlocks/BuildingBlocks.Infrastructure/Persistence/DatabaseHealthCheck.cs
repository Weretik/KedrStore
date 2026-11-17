using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using Npgsql;

namespace BuildingBlocks.Infrastructure.Persistence;

/// <summary>
/// Readiness check: opens a connection with the `Default` connection string and runs `SELECT 1`. Tagged
/// <see cref="ReadyTag"/>, so only `/health/ready` runs it and liveness never restarts an instance because the
/// database is down.
/// </summary>
public sealed class DatabaseHealthCheck(IConfiguration configuration) : IHealthCheck
{
    public const string Name = "database";
    public const string ReadyTag = "ready";

    public async Task<HealthCheckResult> CheckHealthAsync(
        HealthCheckContext context,
        CancellationToken cancellationToken = default)
    {
        var connectionString = configuration.GetConnectionString("Default");
        if (string.IsNullOrWhiteSpace(connectionString))
            return HealthCheckResult.Unhealthy("ConnectionStrings:Default is not configured.");

        try
        {
            await using var connection = new NpgsqlConnection(connectionString);
            await connection.OpenAsync(cancellationToken);
            await using var command = new NpgsqlCommand("SELECT 1", connection);
            await command.ExecuteScalarAsync(cancellationToken);
            return HealthCheckResult.Healthy();
        }
        catch (Exception error) when (error is NpgsqlException or TimeoutException or InvalidOperationException)
        {
            // The reason goes to the health log only; the endpoint answers just "Unhealthy".
            return HealthCheckResult.Unhealthy("PostgreSQL is unreachable.", error);
        }
    }
}
