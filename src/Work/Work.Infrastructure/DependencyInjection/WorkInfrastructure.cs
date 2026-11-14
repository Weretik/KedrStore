using Microsoft.Extensions.DependencyInjection.Extensions;
using Work.Infrastructure.Persistence;

namespace Work.Infrastructure.DependencyInjection;

public static class WorkInfrastructure
{
    public static IServiceCollection AddWorkInfrastructure(
        this IServiceCollection services, IConfiguration configuration)
    {
        var connectionString = configuration.GetConnectionString("Default")
                               ?? throw new InvalidOperationException("Missing ConnectionStrings:Default");

        services.AddDbContext<WorkDbContext>(options => WorkDbContext.Configure(options, connectionString));
        services.AddScoped<IReadWorkDbContext>(sp => sp.GetRequiredService<WorkDbContext>());
        services.AddScoped(typeof(IWorkRepository<>), typeof(WorkEfRepository<>));
        services.TryAddSingleton(TimeProvider.System);
        services.AddScoped<IDatabaseMigrator, DbMigrator<WorkDbContext>>();
        return services;
    }
}
