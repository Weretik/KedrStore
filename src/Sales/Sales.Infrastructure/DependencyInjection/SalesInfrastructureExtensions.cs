namespace Sales.Infrastructure.DependencyInjection;

public static class SalesInfrastructureExtensions
{
    public static IServiceCollection AddSalesInfrastructureServices(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        var connectionString = configuration.GetConnectionString("Default")
                               ?? throw new InvalidOperationException("Missing ConnectionStrings:Default");

        services.Configure<CatalogPricingOptions>(
            configuration.GetSection(CatalogPricingOptions.SectionName));
        services.Configure<SalesTestCustomerOptions>(
            configuration.GetSection(SalesTestCustomerOptions.SectionName));

        services.AddDbContext<SalesDbContext>(options => options.UseNpgsql(connectionString));
        services.AddScoped<IReadSalesDbContext>(sp => sp.GetRequiredService<SalesDbContext>());
        services.AddScoped(typeof(ISalesRepository<>), typeof(SalesEfRepository<>));
        services.AddScoped<IDatabaseMigrator, DbMigrator<SalesDbContext>>();

        services.AddScoped<IPricePolicyProvider, DefaultPricePolicyProvider>();
        services.AddScoped<ICatalogProductReader, CatalogProductReader>();
        services.AddScoped<ISeeder, SalesTestCustomerSeeder>();

        return services;
    }
}
