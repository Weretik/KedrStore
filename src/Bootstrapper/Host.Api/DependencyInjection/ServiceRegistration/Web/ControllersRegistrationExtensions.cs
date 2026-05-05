using Catalog.Api;
using Identity.Api;

namespace Host.Api.DependencyInjection.ServiceRegistration.Web;

public static class ControllersRegistrationExtensions
{
    public static IMvcBuilder AddModuleControllers(this IServiceCollection services)
        => services.AddControllers()
            .AddApplicationPart(typeof(CatalogApiAssemblyMarker).Assembly)
            .AddApplicationPart(typeof(IdentityApiAssemblyMarker).Assembly);
}
