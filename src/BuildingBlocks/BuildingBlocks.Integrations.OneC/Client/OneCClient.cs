using BuildingBlocks.Application.Integrations.OneC.Contracts;
using BuildingBlocks.Application.Integrations.OneC.DTOs;

namespace BuildingBlocks.Integrations.OneC.Client;

public class OneCClient(OneCSoapClientFactory factory) : IOneCClient
{
    public async Task<IReadOnlyList<OneCCategoryDto>> GetCategoriesAsync(string rootCategoryId, CancellationToken ct)
    {
        var client = factory.Create();
        var resp = await client.GetCategoriesAsync(rootCategoryId);
        var list = resp?.Body?.@return;

        if (list is null || list.Count == 0)
            return Array.Empty<OneCCategoryDto>();

        return list.Select(x => new OneCCategoryDto(
                CategoryId: AsString(x.CategoryId),
                ParentId: EmptyToNull(AsString(x.ParentId)),
                CategoryName: AsString(x.CategoryName),
                CategoryPath: AsString(x.CategoryPath)
            ))
            .ToArray();
    }
    public async Task<IReadOnlyList<OneCProductDto>> GetProductDetailsAsync(string rootCategoryId, CancellationToken ct)
    {
        var client = factory.Create();
        var resp = await client.GetProductDetailsAsync(rootCategoryId);
        var list = resp?.Body?.@return;

        if (list is null || list.Count == 0) return Array.Empty<OneCProductDto>();

        return list.Select(x => new OneCProductDto(
                Id: AsString(x.id),
                Name: AsString(x.name),
                CategoryPath: AsString(x.CategoryPath),
                Manufacturer: AsString(x.Manufacturer),
                IsSale: AsBool(x.IsSale),
                IsNew: AsBool(x.IsNew),
                ExportToSite: AsBool(x.ExportToSite),
                QuantityInPack: AsInt(x.QuantityInPack)
            ))
            .ToArray();
    }

    public async Task<IReadOnlyList<OneCStockDto>> GetProductStocksAsync(string rootCategoryId, CancellationToken ct)
    {
        var client = factory.Create();
        var resp = await client.GetProductStocksAsync(rootCategoryId);
        var list = resp?.Body?.@return;

        if (list is null || list.Count == 0) return Array.Empty<OneCStockDto>();

        return list.Select(x => new OneCStockDto(
                Id: AsString(x.id),
                Stock: AsDecimal(x.Stock)
            ))
            .ToArray();
    }

    public async Task<IReadOnlyList<OneCPriceDto>> GetProductPricesAsync(string rootCategoryId, CancellationToken ct)
    {
        var client = factory.Create();
        var resp = await client.GetProductPricesAsync(rootCategoryId);
        var list = resp?.Body?.@return;

        if (list is null || list.Count == 0) return Array.Empty<OneCPriceDto>();

        return list.Select(x => new OneCPriceDto(
                Id: AsString(x.id),
                Price: AsDecimal(x.Price)
            ))
            .ToArray();
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

    private static string? EmptyToNull(string text) => string.IsNullOrWhiteSpace(text) ? null : text;
}
