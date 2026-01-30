namespace Host.Api.DependencyInjection.ServiceCollections.HostServices;

public static class CorsExtensions
{
    public static IServiceCollection AddCorsService(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        var policyName = configuration["Cors:PolicyName"] ?? "Spa";
        var origins = configuration.GetSection("Cors:Origins").Get<string[]>() ?? [];

        services.AddCors(options =>
        {
            options.AddPolicy(policyName, policy =>
            {
                if (origins.Length == 0)
                {
                    return;
                }

                policy
                    .WithOrigins(origins)
                    .AllowAnyHeader()
                    .AllowAnyMethod();
                // cookie-auth:
                // policy.AllowCredentials();
            });
        });

        return services;
    }
}
