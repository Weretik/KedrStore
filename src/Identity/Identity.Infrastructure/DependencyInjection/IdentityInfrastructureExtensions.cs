namespace Identity.Infrastructure.DependencyInjection;

public static class IdentityInfrastructureExtensions
{
    public static IServiceCollection AddIdentityInfrastructureServices(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        services.AddIdentityDbContextServices(configuration);

        return services;
    }
}

