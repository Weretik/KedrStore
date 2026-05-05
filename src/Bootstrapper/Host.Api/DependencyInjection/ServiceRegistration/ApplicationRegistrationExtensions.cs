using BuildingBlocks.Application.Behaviors;
using Host.Api.DependencyInjection.ServiceRegistration.Pipeline;

namespace Host.Api.DependencyInjection.ServiceRegistration;

public static class ApplicationRegistrationsExtensions
{
    public static IServiceCollection AddApplicationRegistrations(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        services.Configure<PerformanceBehaviorOptions>(
            configuration.GetSection(PerformanceBehaviorOptions.SectionName));

        services.AddMediatorPipeline();
        services.AddFluentValidation();

        return services;
    }
}
