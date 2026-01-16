namespace Host.Api.DependencyInjection.ServiceCollections.HostServices;

public static class AuthenticationExtensions
{
    public static IServiceCollection AddAuthentication(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        /*
        services
            .AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
            .AddJwtBearer(options =>
            {
                // TODO: подставь реальные настройки из конфигурации
                // options.Authority = configuration["Auth:Authority"];
                // options.Audience  = configuration["Auth:Audience"];
            });

        services.AddAuthorization(options =>
        {
            options.AddPolicy("CatalogRead", p => p.RequireAuthenticatedUser());
            // options.AddPolicy("CatalogWrite", p => p.RequireAuthenticatedUser());
        });
*/
        return services;
    }
}
