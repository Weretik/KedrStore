using Catalog.Application.DependencyInjection;
using Catalog.Application.Integrations.OneC.Jobs;
using Host.Api.DependencyInjection.ServiceCollections.HostServices;

namespace Host.Api.DependencyInjection.ServiceCollections;

public static class HostServicesExtensions
{
    public static IServiceCollection AddHostServices(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        services.AddIdentityConfiguration(configuration);
        services.AddIdentityInfrastructureServices(configuration);

        services.AddOneCIntegrationServices();

        services.AddInfrastructureServices(configuration);

        services.AddCatalogInfrastructureServices(configuration);
        services.AddCatalogIntegrationOneCServices(configuration);

        services.AddScoped<SyncOneCFullJob>();
        services.AddScoped<SyncOneCPriceTypesJob>();
        services.AddScoped<SyncOneCCategoryJob>();
        services.AddScoped<SyncOneCProductDetailsJob>();
        services.AddScoped<SyncOneCStocksJob>();
        services.AddScoped<SyncOneCPricesJob>();

        services.AddCorsService();

        services.AddMediatorPipeline();
        services.AddFluentValidation();

        services.AddModuleControllers();

        services.AddEndpointsApiExplorer();
        services.AddOpenApi();

        services.AddHealthChecks();
        services.AddSingleton(TimeProvider.System);

        services.AddAuthentication(configuration);

        services.AddProblemDetails();

        return services;
    }
}
