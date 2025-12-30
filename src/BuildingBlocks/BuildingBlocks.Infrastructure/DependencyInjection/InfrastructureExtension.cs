using Application.Catalog.ImportCatalogFromXml;
using Application.Common.Interfaces;
using Application.Common.Notifications;
using BuildingBlocks.Infrastructure.DomainEvents;
using BuildingBlocks.Infrastructure.Services;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace BuildingBlocks.Infrastructure.DependencyInjection;

public static class InfrastructureExtension
{
    public static IServiceCollection AddInfrastructureServices(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddCommonInfrastructure();
        services.AddTelegramInfrastructure(configuration);

        //Регистрация Fake Services
        services.AddScoped<IPermissionService, FakePermissionService>();
        services.AddScoped<ICurrentUserService, FakeCurrentUserService>();

        services.AddScoped<IXmlToJsonConvector, XmlToJsonConvector>();

        return services;
    }

    public static IServiceCollection AddCommonInfrastructure(this IServiceCollection services)
    {
        //Services
        services.AddScoped<IDomainEventContext, EfDomainEventContext>();
        services.AddScoped<IDomainEventDispatcher, MediatorDomainEventDispatcher>();

        return services;
    }
}
