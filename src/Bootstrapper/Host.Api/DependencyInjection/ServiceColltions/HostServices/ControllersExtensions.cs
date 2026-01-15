using Catalog.Api;

namespace Host.Api.DependencyInjection.ServiceColltions.HostServices;

public static class ControllersExtensions
{
    public static IMvcBuilder AddModuleControllers(this IServiceCollection services)
        => services.AddControllers()
            .AddApplicationPart(typeof(CatalogApiAssemblyMarker).Assembly);
    // .AddApplicationPart(typeof(OrdersApiAssemblyMarker).Assembly);
    // .AddApplicationPart(typeof(IdentityApiAssemblyMarker).Assembly);
}
