using Catalog.Application.Integrations.OneC.Jobs;

namespace Host.Api.DependencyInjection.WebApplications.HostPipelines;

public static class RecurringJobs
{
    public static void AddHangfireRecurringJobs(this WebApplication app)
    {
        using var scope = app.Services.CreateScope();

        var cfg = scope.ServiceProvider.GetRequiredService<IConfiguration>();
        var recurring = scope.ServiceProvider.GetRequiredService<IRecurringJobManager>();

        var doors = Required(cfg, "OneC:DoorsRootCategoryId");
        var hardware = Required(cfg, "OneC:HardwareRootCategoryId");

        // ===== PriceType =====
        recurring.AddOrUpdate<SyncOneCPriceTypesJob>(
            "onec-price-types",
            j => j.RunAsync(JobCancellationToken.Null),
            Required(cfg, "OneC:Schedules:PriceTypes"));

        // ===== stocks =====
        recurring.AddOrUpdate<SyncOneCStocksJob>(
            "onec-doors-stocks",
            j => j.RunAsync(doors, JobCancellationToken.Null),
            Required(cfg, "OneC:Schedules:StocksDoors"));

        recurring.AddOrUpdate<SyncOneCStocksJob>(
            "onec-hardware-stocks",
            j => j.RunAsync(hardware, JobCancellationToken.Null),
            Required(cfg, "OneC:Schedules:StocksHardware"));

        // ===== prices =====
        recurring.AddOrUpdate<SyncOneCPricesJob>(
            "onec-doors-prices",
            j => j.RunAsync(doors, JobCancellationToken.Null),
            Required(cfg,"OneC:Schedules:PricesDoors"));

        recurring.AddOrUpdate<SyncOneCPricesJob>(
            "onec-hardware-prices",
            j => j.RunAsync(hardware, JobCancellationToken.Null),
            Required(cfg,"OneC:Schedules:PricesHardware"));

        // ===== products =====
        recurring.AddOrUpdate<SyncOneCProductDetailsJob>(
            "onec-doors-products",
            j => j.RunAsync(doors, JobCancellationToken.Null),
            Required(cfg,"OneC:Schedules:ProductsDoors"));

        recurring.AddOrUpdate<SyncOneCProductDetailsJob>(
            "onec-hardware-products",
            j => j.RunAsync(hardware, JobCancellationToken.Null),
            Required(cfg,"OneC:Schedules:ProductsHardware"));

        // ===== categories =====
        recurring.AddOrUpdate<SyncOneCCategoryJob>(
            "onec-doors-categories",
            j => j.RunAsync(doors, JobCancellationToken.Null),
            Required(cfg,"OneC:Schedules:CategoriesDoors"));

        recurring.AddOrUpdate<SyncOneCCategoryJob>(
            "onec-hardware-categories",
            j => j.RunAsync(hardware, JobCancellationToken.Null),
            Required(cfg,"OneC:Schedules:CategoriesHardware"));
    }

    static string Required(IConfiguration cfg, string key)
        => cfg[key] ?? throw new InvalidOperationException($"Missing config key: {key}");
}
