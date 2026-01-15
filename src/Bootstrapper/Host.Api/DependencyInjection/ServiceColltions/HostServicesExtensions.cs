using Host.Api.DependencyInjection.ServiceColltions.HostServices;

namespace Host.Api.DependencyInjection.ServiceColltions;

public static class HostServicesExtensions
{
    public static IServiceCollection AddHostServices(
        this IServiceCollection services,
        IConfiguration configuration)
    {

        services.AddIdentityConfiguration(configuration);
        services.AddIdentityInfrastructure(configuration);

        services.AddOneCIntegration();

        services.AddInfrastructureServices(configuration);
        services.AddCatalogInfrastructureServices(configuration);

        services.AddHangfire(configuration);

        services.AddMediatorPipeline();
        services.AddValidation();

        services.AddModuleControllers();

        services.AddEndpointsApiExplorer();
        services.AddOpenApi();

        services.AddHealthChecks();
        services.AddSingleton(TimeProvider.System);

        services.AddAuthentication(configuration);

        return services;
    }
}
