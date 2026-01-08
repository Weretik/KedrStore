using System;
using System.Threading;
using System.Threading.Tasks;
using BuildingBlocks.Integrations.OneC.Generated;
using Microsoft.Extensions.Configuration;

namespace BuildingBlocks.Integrations.OneC;

public static class OneCSoapSmokeTest
{
    public static async Task RunAsync(IConfiguration configuration, CancellationToken ct = default)
    {
        var factory = new OneCSoapClientFactory(configuration);
        var client = factory.Create();

        Console.WriteLine(client.Endpoint.Address.Uri);
        Console.WriteLine(client.Endpoint.Binding.GetType().FullName);
        // 1) категории

        var categories = await client.GetCategoriesAsync("000007226");
        Console.WriteLine($"Categories: {categories?.Body?.@return?.Count ?? 0}");


        // 2) остатки
        var stocks = await client.GetProductStocksAsync("000007226");
        Console.WriteLine($"Stocks: {stocks?.Body?.@return?.Count ?? 0}");

        // 3) товары (детали)
        var details = await client.GetProductDetailsAsync("000007226");
        Console.WriteLine($"Details: {details?.Body?.@return?.Count ?? 0}");

        // 4) цены
        var prices = await client.GetProductPricesAsync("000007226");
        Console.WriteLine($"Prices: {prices?.Body?.@return?.Count ?? 0}");
    }
}
