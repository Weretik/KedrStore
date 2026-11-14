using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using FluentValidation;
using Microsoft.AspNetCore.Mvc;
using Work.Api.Activity;
using Work.Infrastructure.DependencyInjection;
namespace Work.Api.DependencyInjection;

/// <summary>Registers the Work module: Application validators, Infrastructure, Contracts implementations, and controllers.</summary>
public static class WorkModule
{
    public static IServiceCollection AddWorkModule(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddWorkInfrastructureServices(configuration);
        // Every admin change in any module is logged by this global filter.
        services.Configure<MvcOptions>(options => options.Filters.Add<WorkActivityFilter>());
        services.AddValidatorsFromAssembly(typeof(Work.Application.WorkApplicationAssemblyMarker).Assembly, includeInternalTypes: true);
        services.AddControllers().AddApplicationPart(typeof(WorkApiAssemblyMarker).Assembly);

        return services;
    }
}
