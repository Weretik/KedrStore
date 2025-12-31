using BuildingBlocks.Application.Contracts;
using BuildingBlocks.Application.Notifications;
using BuildingBlocks.Infrastructure.DomainEvents;
using BuildingBlocks.Infrastructure.Services;
using Catalog.Application.Features.Import.Queries.ImportCatalogFromXml;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace BuildingBlocks.Infrastructure.DependencyInjection;

public static class InfrastructureDependencyInjection
{
    public static IServiceCollection AddInfrastructureServices(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddTelegramInfrastructure(configuration);

        services.AddScoped<IDomainEventContext, EfDomainEventContext>();
        services.AddScoped<IDomainEventDispatcher, MediatorDomainEventDispatcher>();

        //Регистрация Fake Services
        services.AddScoped<IPermissionService, FakePermissionService>();
        services.AddScoped<ICurrentUserService, FakeCurrentUserService>();

        services.AddScoped<IXmlToJsonConvector, XmlToJsonConvector>();

        return services;
    }
}
