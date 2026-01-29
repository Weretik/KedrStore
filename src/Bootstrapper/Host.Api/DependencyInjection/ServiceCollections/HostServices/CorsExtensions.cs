namespace Host.Api.DependencyInjection.ServiceCollections.HostServices;

public static class CorsExtensions
{
    public static IServiceCollection AddCorsService(this IServiceCollection services)
    {

        services.AddCors(options =>
        {
            options.AddPolicy("SpaDev", policy =>
                    policy
                        .WithOrigins(
                            "http://localhost:4200",
                            "https://classkedr.com.ua"
                            )
                        .AllowAnyHeader()
                        .AllowAnyMethod()
                // cookies/auth
                // .AllowCredentials()
            );
        });
        return services;
    }
}
