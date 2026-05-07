namespace Host.Api.DependencyInjection.ServiceRegistration.Web;

public static class CorsRegistrationExtensions
{
    public static IServiceCollection AddCorsService(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        var allowedOrigins = configuration
            .GetSection("Cors:AllowedOrigins")
            .Get<string[]>()?
            .Select(origin => origin.Trim().TrimEnd('/'))
            .Where(origin => !string.IsNullOrWhiteSpace(origin))
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .ToArray() ?? [];

        services.AddCors(options =>
        {
            options.AddPolicy("Frontend", policy =>
            {
                if (allowedOrigins.Length == 0)
                {
                    throw new InvalidOperationException("Cors:AllowedOrigins is empty");
                }

                policy.WithOrigins(allowedOrigins)
                    .SetIsOriginAllowedToAllowWildcardSubdomains()
                    .AllowAnyHeader()
                    .WithMethods("GET", "POST", "PUT", "PATCH", "DELETE", "OPTIONS")
                    .AllowCredentials()
                    .SetPreflightMaxAge(TimeSpan.FromMinutes(30));
            });
        });

        return services;
    }
}
