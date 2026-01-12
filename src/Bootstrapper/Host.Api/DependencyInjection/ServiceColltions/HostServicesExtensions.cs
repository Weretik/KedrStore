namespace Host.Api.DependencyInjection.ServiceColltions;

public static class HostServicesExtensions
{
    public static IServiceCollection AddHostServices(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        services
            .AddIdentityConfiguration(configuration)
            .AddIdentityInfrastructure(configuration);

        services
            .AddOneCIntegration()
            .AddInfrastructureServices(configuration)
            .AddCatalogInfrastructureServices(configuration);

        services
            .AddMediatorPipeline()
            .AddValidation();

        services
            .AddModuleControllers();

        services
            .AddEndpointsApiExplorer()
            .AddOpenApi();

        services.AddHealthChecks();
        services.AddSingleton(TimeProvider.System);

        services.AddHostAuthentication(configuration);

        return services;
    }
}
