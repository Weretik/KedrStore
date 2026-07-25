using System;
using System.Threading;
using System.Threading.Tasks;
using BuildingBlocks.Integrations.OneC.Factory;
using BuildingBlocks.Integrations.OneC.Generated;
using Microsoft.Extensions.Configuration;

namespace BuildingBlocks.Integrations.OneC;

public static class OneCSoapSmokeTest
{
    public static async Task RunAsync(IConfiguration configuration, CancellationToken ct = default)
    {
        var factory = new OneCSoapClientFactory(configuration);
        var client = factory.Create();
        var rootCategoryId = configuration["OneC:DoorsRootCategoryId"] ?? "000007226";

        Console.WriteLine(client.Endpoint.Binding.GetType().FullName);

        var categories = await client.GetCategoriesAsync(rootCategoryId);
        Console.WriteLine($"Categories: {categories?.@return?.Length ?? 0}");

        var stocks = await client.GetProductStocksAsync(rootCategoryId);
        Console.WriteLine($"Stocks: {stocks?.@return?.Length ?? 0}");

        var details = await client.GetProductDetailsAsync(rootCategoryId);
        Console.WriteLine($"Details: {details?.@return?.Length ?? 0}");

        var prices = await client.GetProductPricesAsync(rootCategoryId);
        Console.WriteLine($"Prices: {prices?.@return?.Length ?? 0}");

        var priceTypes = await client.GetPriceTypesAsync();
        Console.WriteLine($"PriceTypes: {priceTypes?.@return?.Length ?? 0}");

        var counterparties = await client.GetCounterpartiesAsync();
        Console.WriteLine($"Counterparties: {counterparties?.@return?.Length ?? 0}");

        var counterpartyCategoryPriceTypes = await client.GetCounterpartyCategoryPriceTypesAsync();
        Console.WriteLine($"CounterpartyCategoryPriceTypes: {counterpartyCategoryPriceTypes?.@return?.Length ?? 0}");

        var runCreateSiteRequest = bool.TryParse(configuration["OneCSoap:RunCreateSiteRequest"], out var enabled) && enabled;
        if (!runCreateSiteRequest)
        {
            Console.WriteLine("CreateSiteRequest: skipped (set OneCSoap:RunCreateSiteRequest=true to execute write smoke test)");
            return;
        }

        var requestData = BuildCreateSiteRequest(configuration);
        var createSiteRequest = await client.CreateSiteRequestAsync(requestData);
        Console.WriteLine($"CreateSiteRequest: completed={createSiteRequest is not null}");
    }

    private static RequestData BuildCreateSiteRequest(IConfiguration configuration)
    {
        var counterpartyId = configuration["OneCSoap:CreateSiteRequest:CounterpartyId"] ?? "test-counterparty";
        var comment = configuration["OneCSoap:CreateSiteRequest:Comment"] ?? "SOAP smoke test";
        var orderId = configuration["OneCSoap:CreateSiteRequest:OrderId"] ?? $"smoke-{DateTime.UtcNow:yyyyMMddHHmmss}";
        var productId = configuration["OneCSoap:CreateSiteRequest:ProductId"] ?? "1";

        return new RequestData
        {
            CounterpartyId = counterpartyId,
            Comment = comment,
            OrderId = orderId,
            Date = DateTime.UtcNow,
            Items =
            [
                new Item
                {
                    ProductId = productId,
                    Quantity = "1",
                    Amount = 0m
                }
            ]
        };
    }
}
