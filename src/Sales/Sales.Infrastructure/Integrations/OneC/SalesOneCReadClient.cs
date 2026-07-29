using System.Globalization;
using BuildingBlocks.Integrations.OneC.Factory;
using Sales.Application.Integrations.OneC.Contracts;
using Sales.Application.Integrations.OneC.DTOs;

namespace Sales.Infrastructure.Integrations.OneC;

internal sealed class SalesOneCReadClient(OneCSoapClientFactory factory) : ISalesOneCReadClient
{
    public async Task<IReadOnlyList<OneCCounterpartyDto>> GetCounterpartiesAsync(CancellationToken cancellationToken)
    {
        var client = factory.Create();
        var response = await client.GetCounterpartiesAsync();
        var rows = response?.@return;

        if (rows is null || rows.Length == 0)
            return [];

        return rows.Select(row => new OneCCounterpartyDto(
            CounterpartyId: AsRequiredString(row.CounterpartyId),
            CounterpartyName: AsRequiredString(row.CounterpartyName),
            Email: AsRequiredString(row.Email),
            Phone: AsNullableString(row.Phone),
            DefaultPriceTypeId: AsId(row.DefaultPriceTypeId)))
            .ToArray();
    }

    public async Task<IReadOnlyList<OneCCounterpartyCategoryPriceTypeDto>> GetCounterpartyCategoryPriceTypesAsync(CancellationToken cancellationToken)
    {
        var client = factory.Create();
        var response = await client.GetCounterpartyCategoryPriceTypesAsync();
        var rows = response?.@return;

        if (rows is null || rows.Length == 0)
            return [];

        return rows.Select(row => new OneCCounterpartyCategoryPriceTypeDto(
            CounterpartyId: AsRequiredString(row.CounterpartyId),
            CategoryId: AsId(row.CategoryId),
            PriceTypeId: AsId(row.PriceTypeId)))
            .ToArray();
    }

    private static string AsRequiredString(string? value) => value?.Trim() ?? string.Empty;

    private static string? AsNullableString(string? value)
    {
        var trimmed = value?.Trim();
        return string.IsNullOrWhiteSpace(trimmed) ? null : trimmed;
    }

    private static int AsId(string? value)
    {
        var text = value?.Trim();

        if (string.IsNullOrWhiteSpace(text))
            return 0;

        if (int.TryParse(text.TrimStart('0'), CultureInfo.InvariantCulture, out var id))
            return id;

        return 0;
    }
}
