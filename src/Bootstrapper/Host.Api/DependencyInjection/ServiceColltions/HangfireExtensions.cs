using Catalog.Application.Integrations.OneC.Jobs;

namespace Host.Api.DependencyInjection.ServiceColltions;

public static class HangfireExtensions
{
    public static IServiceCollection AddHangfireServices(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddHangfire(cfg =>
        {
#pragma warning disable CS0618 // Type or member is obsolete
            cfg.UseSimpleAssemblyNameTypeSerializer()
                .UseRecommendedSerializerSettings()
                .UsePostgreSqlStorage(configuration.GetConnectionString("Default")
                                      ?? throw new InvalidOperationException(
                                          "ConnectionStrings:Default is missing"));
#pragma warning restore CS0618 // Type or member is obsolete

        });

        services.AddHangfireServer(options =>
        {
            options.ServerName = $"{Environment.MachineName}:hangfire";
            options.WorkerCount = Math.Min(Environment.ProcessorCount * 5, 50);
        });

        services.AddScoped<SyncOneCCategoryJob>();
        services.AddScoped<SyncOneCProductDetailsJob>();
        services.AddScoped<SyncOneCStocksJob>();
        services.AddScoped<SyncOneCPricesJob>();

        return services;
    }
}
