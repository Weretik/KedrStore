using BuildingBlocks.Application.Integrations.OneC.Contracts;
using BuildingBlocks.Application.Integrations.OneC.DTOs;

namespace BuildingBlocks.Integrations.OneC.Client;

public class OneCClient(OneCSoapClientFactory factory) : IOneCClient
{
    public async Task<IReadOnlyList<OneCPriceTypeDto>> GetPriceTypesAsync(CancellationToken cancellationToken)
    {
        var client = factory.Create();
        var resp = await client.GetPriceTypesAsync();
        var list = resp?.Body?.@return;

        if (list is null || list.Count == 0)
            return [];

        return list.Select(x => new OneCPriceTypeDto(
            PriceTypeId: AsId(x.PriceTypeId),
            PriceTypeName: AsString(x.PriceTypeName))
        ).ToArray();;

    }
    public async Task<IReadOnlyList<OneCCategoryDto>> GetCategoriesAsync(string rootCategoryId, CancellationToken cancellationToken)
    {
        var client = factory.Create();
        var resp = await client.GetCategoriesAsync(rootCategoryId);
        var list = resp?.Body?.@return;

        if (list is null || list.Count == 0)
            return [];

        return list.Select(x => new OneCCategoryDto(
                CategoryId: AsId(x.CategoryId),
                ParentId: AsIdOrNull(x.ParentId),
                CategoryName: AsString(x.CategoryName),
                CategoryPath: AsString(x.CategoryPath))
        ).ToArray();
    }
    public async Task<IReadOnlyList<OneCProductDto>> GetProductDetailsAsync(string rootCategoryId, CancellationToken cancellationToken)
    {
        var client = factory.Create();
        var resp = await client.GetProductDetailsAsync(rootCategoryId);
        var list = resp?.Body?.@return;

        if (list is null || list.Count == 0)
            return [];

        return list.Select(x => new OneCProductDto(
                Id: AsId(x.id),
                Name: AsString(x.name),
                CategoryPath: AsString(x.CategoryPath),
                Manufacturer: AsString(x.Manufacturer),
                IsSale: AsBool(x.IsSale),
                IsNew: AsBool(x.IsNew),
                ExportToSite: AsBool(x.ExportToSite),
                QuantityInPack: AsInt(x.QuantityInPack))
        ).ToArray();
    }

    public async Task<IReadOnlyList<OneCStockDto>> GetProductStocksAsync(string rootCategoryId, CancellationToken cancellationToken)
    {
        var client = factory.Create();
        var resp = await client.GetProductStocksAsync(rootCategoryId);
        var list = resp?.Body?.@return;

        if (list is null || list.Count == 0)
            return [];

        return list.Select(x => new OneCStockDto(
                Id: AsId(x.id),
                Stock: AsDecimal(x.Stock))
        ).ToArray();
    }

    public async Task<IReadOnlyList<OneCPriceDto>> GetProductPricesAsync(string rootCategoryId, CancellationToken cancellationToken)
    {
        var client = factory.Create();
        var resp = await client.GetProductPricesAsync(rootCategoryId);
        var list = resp?.Body?.@return;

        if (list is null || list.Count == 0)
            return [];

        return list.Select(x => new OneCPriceDto(
                ProductId: AsId(x.id),
                PriceTypeId: AsId(x.PriceTypeId),
                Price: AsDecimal(x.Price))
        ).ToArray();
    }

    private static int AsId(object? value)
    {
        var s = value?.ToString()?.Trim();

        if (string.IsNullOrEmpty(s))
            return 0;

        return int.TryParse(s.TrimStart('0'), out var id) ? id : 0;
    }
    private static int? AsIdOrNull(object? value)
    {
        var s = value?.ToString()?.Trim();

        if (string.IsNullOrEmpty(s))
            return null;

        return int.TryParse(s.TrimStart('0'), out var id) ? id : 0;
    }
    private static string AsString(object? value) => value?.ToString()?.Trim() ?? string.Empty;

    private static bool AsBool(object? value)
        => value switch
        {
            bool b => b,
            string s => s.Equals("true", StringComparison.OrdinalIgnoreCase) || s == "1",
            _ => bool.TryParse(value?.ToString(), out var b) && b
        };

    private static int AsInt(object? value)
        => int.TryParse(value?.ToString(), out var i) ? i : 0;

    private static decimal AsDecimal(object? value)
        => decimal.TryParse(value?.ToString(), out var num) ? num : 0m;
}
