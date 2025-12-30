using Microsoft.Extensions.Configuration;

namespace BuildingBlocks.Integrations.OneC;

public static class OneCSoapSmokeTest
{
    public static async Task RunAsync(IConfiguration configuration, CancellationToken ct = default)
    {
        var factory = new OneCSoapClientFactory(configuration);
        var client = factory.Create();

        // 1) категории
        var categories = await client.GetCategoriesAsync();
        Console.WriteLine($"Categories: {categories?.Body?.@return?.Count ?? 0}");

        // 2) остатки
        var stocks = await client.GetProductStocksAsync();
        Console.WriteLine($"Stocks: {stocks?.Body?.@return?.Count ?? 0}");

        // 3) товары (детали)
        var details = await client.GetProductDetailsAsync();
        Console.WriteLine($"Details: {details?.Body?.@return?.Count ?? 0}");

        // 4) цены
        var prices = await client.GetProductPricesAsync();
        Console.WriteLine($"Prices: {prices?.Body?.@return?.Count ?? 0}");
    }
}
