using DotNet.Testcontainers.Builders;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Sales.Infrastructure.DataBase;
using Testcontainers.PostgreSql;

namespace IntegrationTests.TestSupport.Database;

[CollectionDefinition(Name)]
public sealed class SalesPostgresCollection : ICollectionFixture<SalesPostgresFixture>
{
    public const string Name = "Sales PostgreSQL";
}

public sealed class SalesPostgresFixture : IAsyncLifetime
{
    public const string ExternalConnectionStringEnvironmentVariable = "KEDR_SALES_TEST_POSTGRES_CONNECTION";

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
                .WithDatabase("kedr_sales_tests")
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

    public SalesDbContext CreateContext(params IInterceptor[] interceptors)
    {
        if (_container is null && string.IsNullOrWhiteSpace(_externalConnectionString))
            throw new InvalidOperationException(UnavailableReason ?? "PostgreSQL fixture is not initialized.");

        var options = new DbContextOptionsBuilder<SalesDbContext>()
            .UseNpgsql(ConnectionString)
            .AddInterceptors(interceptors)
            .Options;
        return new SalesDbContext(options);
    }

    public void SkipIfUnavailable()
        => Skip.If(UnavailableReason is not null, UnavailableReason);

    public async Task DisposeAsync()
    {
        if (_container is not null)
            await _container.DisposeAsync();
    }
}
