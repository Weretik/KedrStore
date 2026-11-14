using DotNet.Testcontainers.Builders;
using Microsoft.EntityFrameworkCore;
using Scheduling.Infrastructure.Persistence;
using Testcontainers.PostgreSql;

namespace IntegrationTests.Scheduling.Postgres;

[CollectionDefinition(Name)]
public sealed class SchedulingPostgresCollection : ICollectionFixture<SchedulingPostgresFixture>
{
    public const string Name = "Scheduling PostgreSQL";
}

/// <summary>The Scheduling schema on real PostgreSQL, which accepts only UTC instants in <c>timestamptz</c>.</summary>
public sealed class SchedulingPostgresFixture : IAsyncLifetime
{
    public const string ExternalConnectionStringEnvironmentVariable = "KEDR_SCHEDULING_TEST_POSTGRES_CONNECTION";

    private PostgreSqlContainer? _container;
    private string? _externalConnectionString;

    public string? UnavailableReason { get; private set; }

    public string ConnectionString => _externalConnectionString ?? _container?.GetConnectionString()
        ?? throw new InvalidOperationException(UnavailableReason ?? "PostgreSQL fixture is not initialized.");

    public async Task InitializeAsync()
    {
        _externalConnectionString = Environment.GetEnvironmentVariable(ExternalConnectionStringEnvironmentVariable);
        if (!string.IsNullOrWhiteSpace(_externalConnectionString))
        {
            await using var externalDb = CreateContext();
            await externalDb.Database.MigrateAsync();
            return;
        }

        try
        {
            _container = new PostgreSqlBuilder("postgres:18-alpine")
                .WithDatabase("kedr_scheduling_tests")
                .WithUsername("kedr_tests")
                .WithPassword("kedr_tests_password")
                .Build();

            await _container.StartAsync();
            await using var db = CreateContext();
            await db.Database.MigrateAsync();
        }
        catch (DockerUnavailableException exception)
        {
            if (string.Equals(Environment.GetEnvironmentVariable("CI"), "true", StringComparison.OrdinalIgnoreCase))
                throw;

            UnavailableReason = $"Docker is unavailable; PostgreSQL integration test skipped. {exception.Message}";
            if (_container is not null)
            {
                await _container.DisposeAsync();
                _container = null;
            }
        }
    }

    public SchedulingDbContext CreateContext()
    {
        if (_container is null && string.IsNullOrWhiteSpace(_externalConnectionString))
            throw new InvalidOperationException(UnavailableReason ?? "PostgreSQL fixture is not initialized.");

        var options = new DbContextOptionsBuilder<SchedulingDbContext>();
        SchedulingDbContext.Configure(options, ConnectionString);
        return new SchedulingDbContext(options.Options);
    }

    public void SkipIfUnavailable()
        => Skip.If(UnavailableReason is not null, UnavailableReason);

    public async Task DisposeAsync()
    {
        if (_container is not null)
            await _container.DisposeAsync();
    }
}
