using Catalog.Application.Integrations.OneC.Jobs;

namespace Host.Api.DependencyInjection.WebApplications;

public static class RecurringJobs
{
    public static void AddRecurringJobs(this WebApplication app)
    {
        using var scope = app.Services.CreateScope();

        var cfg = scope.ServiceProvider.GetRequiredService<IConfiguration>();
        var recurring = scope.ServiceProvider.GetRequiredService<IRecurringJobManager>();

        var doors = Required(cfg, "OneC:DoorsRootCategoryId");
        var hardware = Required(cfg, "OneC:HardwareRootCategoryId");

        var stocksDoorsCron = Required(cfg, "OneC:Schedules:StocksDoors");
        var stocksHardwareCron = Required(cfg, "OneC:Schedules:StocksHardware");

        var pricesDoorsCron = Required(cfg,"OneC:Schedules:PricesDoors");
        var pricesHardwareCron = Required(cfg,"OneC:Schedules:PricesHardware");

        var productsDoorsCron = Required(cfg,"OneC:Schedules:ProductsDoors");
        var productsHardwareCron = Required(cfg,"OneC:Schedules:ProductsHardware");

        var categoriesDoorsCron = Required(cfg,"OneC:Schedules:CategoriesDoors");
        var categoriesHardwareCron = Required(cfg,"OneC:Schedules:CategoriesHardware");

        // ===== stocks =====
        recurring.AddOrUpdate<SyncOneCStocksJob>(
            "onec-doors-stocks",
            j => j.RunAsync(doors, CancellationToken.None),
            stocksDoorsCron);

        recurring.AddOrUpdate<SyncOneCStocksJob>(
            "onec-hardware-stocks",
            j => j.RunAsync(hardware, CancellationToken.None),
            stocksHardwareCron);

        // ===== prices =====
        recurring.AddOrUpdate<SyncOneCPricesJob>(
            "onec-doors-prices",
            j => j.RunAsync(doors, CancellationToken.None),
            pricesDoorsCron);

        recurring.AddOrUpdate<SyncOneCPricesJob>(
            "onec-hardware-prices",
            j => j.RunAsync(hardware, CancellationToken.None),
            pricesHardwareCron);

        // ===== products =====
        recurring.AddOrUpdate<SyncOneCProductDetailsJob>(
            "onec-doors-products",
            j => j.RunAsync(doors, CancellationToken.None),
            productsDoorsCron);

        recurring.AddOrUpdate<SyncOneCProductDetailsJob>(
            "onec-hardware-products",
            j => j.RunAsync(hardware, CancellationToken.None),
            productsHardwareCron);

        // ===== categories =====
        recurring.AddOrUpdate<SyncOneCCategoryJob>(
            "onec-doors-categories",
            j => j.RunAsync(doors, CancellationToken.None),
            categoriesDoorsCron);

        recurring.AddOrUpdate<SyncOneCCategoryJob>(
            "onec-hardware-categories",
            j => j.RunAsync(hardware, CancellationToken.None),
            categoriesHardwareCron);
    }

    static string Required(IConfiguration cfg, string key)
        => cfg[key] ?? throw new InvalidOperationException($"Missing config key: {key}");
}
