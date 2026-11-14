using FluentValidation;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Work.Api.Endpoints.Activity;
using Work.Api.Endpoints.Report;
using Work.Api.Endpoints.Tasks;
using Work.Infrastructure.DependencyInjection;

namespace Work.Api.DependencyInjection;

/// <summary>Registers the Work module: Application validators, Infrastructure, endpoints, and the activity log.</summary>
public static class WorkModule
{
    public static IServiceCollection AddWorkModule(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddWorkInfrastructure(configuration);
        services.AddValidatorsFromAssembly(typeof(Work.Application.WorkApplicationAssemblyMarker).Assembly, includeInternalTypes: true);

        return services;
    }

    public static IEndpointRouteBuilder MapWorkEndpoints(this IEndpointRouteBuilder endpoints)
    {
        endpoints.MapTasksEndpoints();
        endpoints.MapReportEndpoints();
        return endpoints;
    }

    /// <summary>Logs every successful admin change of any module; goes after authorization.</summary>
    public static IApplicationBuilder UseWorkActivity(this IApplicationBuilder app) =>
        app.UseMiddleware<WorkActivityMiddleware>();
}
