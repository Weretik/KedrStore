using BuildingBlocks.Infrastructure.Migrations;
using BuildingBlocks.Infrastructure.Seeding;
using Host.Jobs;
using Microsoft.Extensions.Logging.Abstractions;

namespace UnitTests;

public sealed class DatabaseMigrationJobTests
{
    [Fact]
    public async Task RunAsync_MigratesInDeterministicOrderBeforeSeeding()
    {
        var events = new List<string>();
        var job = CreateJob(
            [new ZetaMigrator(events), new AlphaMigrator(events)],
            [new RecordingSeeder(events)]);

        await job.RunAsync(CancellationToken.None);

        Assert.Equal(["migrate:alpha", "migrate:zeta", "seed"], events);
    }

    [Fact]
    public async Task RunAsync_StopsBeforeSeedingWhenMigrationFails()
    {
        var events = new List<string>();
        var job = CreateJob(
            [new FailingMigrator(events)],
            [new RecordingSeeder(events)]);

        await Assert.ThrowsAsync<InvalidOperationException>(() => job.RunAsync(CancellationToken.None));

        Assert.Equal(["migrate:fail"], events);
    }

    private static DatabaseMigrationJob CreateJob(
        IEnumerable<IDatabaseMigrator> migrators,
        IEnumerable<ISeeder> seeders)
        => new(
            migrators,
            seeders,
            EmptyServiceProvider.Instance,
            NullLogger<DatabaseMigrationJob>.Instance);

    private sealed class AlphaMigrator(List<string> events) : IDatabaseMigrator
    {
        public Task MigrateAsync(CancellationToken cancellationToken = default)
        {
            events.Add("migrate:alpha");
            return Task.CompletedTask;
        }
    }

    private sealed class ZetaMigrator(List<string> events) : IDatabaseMigrator
    {
        public Task MigrateAsync(CancellationToken cancellationToken = default)
        {
            events.Add("migrate:zeta");
            return Task.CompletedTask;
        }
    }

    private sealed class FailingMigrator(List<string> events) : IDatabaseMigrator
    {
        public Task MigrateAsync(CancellationToken cancellationToken = default)
        {
            events.Add("migrate:fail");
            throw new InvalidOperationException("Expected test failure.");
        }
    }

    private sealed class RecordingSeeder(List<string> events) : ISeeder
    {
        public Task SeedAsync(IServiceProvider services, CancellationToken cancellationToken = default)
        {
            events.Add("seed");
            return Task.CompletedTask;
        }
    }

    private sealed class EmptyServiceProvider : IServiceProvider
    {
        public static readonly EmptyServiceProvider Instance = new();

        public object? GetService(Type serviceType) => null;
    }
}
