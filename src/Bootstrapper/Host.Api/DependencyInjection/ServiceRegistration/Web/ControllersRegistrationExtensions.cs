using Catalog.Api;
using Content.Api;
using Identity.Api;
using Media.Api;
using Sales.Api;
using Work.Api;
using Work.Api.Activity;

namespace Host.Api.DependencyInjection.ServiceRegistration.Web;

public static class ControllersRegistrationExtensions
{
    public static IMvcBuilder AddModuleControllers(this IServiceCollection services)
        => services.AddControllers(options => options.Filters.Add<WorkActivityFilter>())
            .AddApplicationPart(typeof(CatalogApiAssemblyMarker).Assembly)
            .AddApplicationPart(typeof(IdentityApiAssemblyMarker).Assembly)
            .AddApplicationPart(typeof(SalesApiAssemblyMarker).Assembly)
            .AddApplicationPart(typeof(ContentApiAssemblyMarker).Assembly)
            .AddApplicationPart(typeof(MediaApiAssemblyMarker).Assembly)
            .AddApplicationPart(typeof(WorkApiAssemblyMarker).Assembly);
}
