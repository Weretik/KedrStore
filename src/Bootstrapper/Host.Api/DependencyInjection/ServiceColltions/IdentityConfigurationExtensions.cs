namespace Host.Api.DependencyInjection.ServiceColltions;

public static class IdentityConfigurationExtensions
{
    public static IServiceCollection AddIdentityConfiguration(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        services.Configure<AdminUserConfig>(
            configuration.GetSection("Identity:AdminUser"));

        return services;
    }
}
