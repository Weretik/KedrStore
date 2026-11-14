using FluentValidation;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Scheduling.Api.Endpoints.Tasks;
using Scheduling.Api.Endpoints.Tick;
using Scheduling.Api.Endpoints.Tick.RunSchedulerTick;
using Scheduling.Infrastructure.DependencyInjection;

namespace Scheduling.Api.DependencyInjection;

/// <summary>Registers the Scheduling module: Application validators, Infrastructure and the tick's caller check.</summary>
public static class SchedulingModule
{
    public static IServiceCollection AddSchedulingModule(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddSchedulingInfrastructure(configuration);
        services.AddValidatorsFromAssembly(typeof(Scheduling.Application.SchedulingApplicationAssemblyMarker).Assembly,
            includeInternalTypes: true);
        services.Configure<SchedulerTickOptions>(configuration.GetSection(SchedulerTickOptions.Section));
        services.AddSingleton<ISchedulerCallerCheck, GoogleSchedulerCallerCheck>();
        return services;
    }

    /// <summary>Maps «Фонові задачі» and the scheduler tick.</summary>
    public static IEndpointRouteBuilder MapSchedulingEndpoints(this IEndpointRouteBuilder endpoints) =>
        endpoints.MapScheduledTasksEndpoints().MapSchedulerTick();
}
