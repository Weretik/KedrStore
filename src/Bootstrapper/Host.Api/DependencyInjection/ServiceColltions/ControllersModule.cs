using Catalog.Api;

namespace Host.Api.DependencyInjection.ServiceColltions;

public static class ControllersModule
{
    public static IMvcBuilder AddModuleControllers(this IServiceCollection services)
        => services.AddControllers()
            .AddApplicationPart(typeof(CatalogApiAssemblyMarker).Assembly);
    // .AddApplicationPart(typeof(OrdersApiAssemblyMarker).Assembly);
    // .AddApplicationPart(typeof(IdentityApiAssemblyMarker).Assembly);
}
