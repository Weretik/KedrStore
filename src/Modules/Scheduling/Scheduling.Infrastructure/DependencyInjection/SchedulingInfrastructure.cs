using BuildingBlocks.Infrastructure.Migrations;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Scheduling.Application.Abstractions;
using Scheduling.Application.Features.Runs;
using Scheduling.Contracts.Runs;
using Scheduling.Infrastructure.Integrations.CloudRun;
using Scheduling.Infrastructure.Integrations.Outbox;
using Scheduling.Infrastructure.Persistence;

namespace Scheduling.Infrastructure.DependencyInjection;

public static class SchedulingInfrastructure
{
    /// <summary>
    /// DbContext, migrator and the run journal (both hosts); the Cloud Run launcher and the tick's outbox work are used
    /// only by the API's tick.
    /// </summary>
    public static IServiceCollection AddSchedulingInfrastructure(this IServiceCollection services, IConfiguration configuration)
    {
        var connectionString = configuration.GetConnectionString("Default")
                               ?? throw new InvalidOperationException("Missing ConnectionStrings:Default");

        services.AddDbContext<SchedulingDbContext>(options => SchedulingDbContext.Configure(options, connectionString));
        services.AddScoped<ISchedulingDbContext>(sp => sp.GetRequiredService<SchedulingDbContext>());
        services.AddScoped<IDatabaseMigrator, DbMigrator<SchedulingDbContext>>();
        services.AddScoped<IScheduledTaskRunJournal, ScheduledTaskRunJournal>();
        services.TryAddSingleton(TimeProvider.System);

        services.Configure<CloudRunJobOptions>(configuration.GetSection(CloudRunJobOptions.Section));
        services.AddHttpClient<ITaskJobLauncher, CloudRunTaskJobLauncher>(client => client.Timeout = TimeSpan.FromSeconds(30));
        services.AddScoped<ITickWork, OutboxTickWork>();
        return services;
    }
}
