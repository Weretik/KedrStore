using IntegrationTests.TestSupport.Api;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Scheduling.Api.Endpoints.Tick.RunSchedulerTick;
using Scheduling.Application.Abstractions;
using Scheduling.Infrastructure.Persistence;

namespace IntegrationTests.Scheduling.Api;

/// <summary>
/// The API with the Scheduling module on SQLite in memory, a fake job launcher and a caller check that accepts the
/// token <see cref="SchedulerToken"/>.
/// </summary>
internal sealed class SchedulingTestHost : IDisposable
{
    public const string SchedulerToken = "scheduler-token";

    private readonly SqliteConnection _connection = new("DataSource=:memory:");

    public SchedulingTestHost(bool tickConfigured = true)
    {
        _connection.Open();
        Factory = new WebApplicationFactory<Program>().WithWebHostBuilder(builder =>
        {
            builder.UseEnvironment("Development");
            if (tickConfigured)
            {
                builder.UseSetting("Scheduler:Audience", "https://api.test/internal/scheduler/tick");
                builder.UseSetting("Scheduler:SchedulerServiceAccount", "scheduler@test.iam.gserviceaccount.com");
            }

            builder.ConfigureServices(services =>
            {
                services.AddTestRoleAuthentication();
                var options = new DbContextOptionsBuilder<SchedulingDbContext>().UseSqlite(_connection)
                    .ReplaceService<IModelCustomizer, SqliteInstantsCustomizer>().Options;
                services.RemoveAll<SchedulingDbContext>();
                services.AddScoped(_ => new SchedulingDbContext(options));
                services.RemoveAll<ITaskJobLauncher>();
                services.AddSingleton<ITaskJobLauncher>(Launcher);
                services.RemoveAll<ITickWork>();
                services.RemoveAll<ISchedulerCallerCheck>();
                services.AddSingleton<ISchedulerCallerCheck, FakeCallerCheck>();
            });
        });
        using var scope = Factory.Services.CreateScope();
        scope.ServiceProvider.GetRequiredService<SchedulingDbContext>().Database.EnsureCreated();
    }

    public WebApplicationFactory<Program> Factory { get; }
    public FakeLauncher Launcher { get; } = new();

    public void Dispose()
    {
        Factory.Dispose();
        _connection.Dispose();
    }

    internal sealed class FakeLauncher : ITaskJobLauncher
    {
        public List<IReadOnlyList<TaskLaunch>> Launches { get; } = [];

        public Task<string?> LaunchAsync(IReadOnlyList<TaskLaunch> launches, CancellationToken cancellationToken)
        {
            Launches.Add(launches);
            return Task.FromResult<string?>(null);
        }
    }

    private sealed class FakeCallerCheck : ISchedulerCallerCheck
    {
        public Task<bool> IsSchedulerAsync(string token, SchedulerTickOptions options) =>
            Task.FromResult(token == SchedulerToken);
    }

    /// <summary>SQLite compares DateTimeOffset only as numbers, the way PostgreSQL compares timestamptz.</summary>
    private sealed class SqliteInstantsCustomizer(ModelCustomizerDependencies dependencies) : RelationalModelCustomizer(dependencies)
    {
        public override void Customize(ModelBuilder modelBuilder, DbContext context)
        {
            base.Customize(modelBuilder, context);
            foreach (var property in modelBuilder.Model.GetEntityTypes().SelectMany(x => x.GetProperties())
                         .Where(x => x.ClrType == typeof(DateTimeOffset) || x.ClrType == typeof(DateTimeOffset?)))
                property.SetValueConverter(new DateTimeOffsetToBinaryConverter());
        }
    }
}
