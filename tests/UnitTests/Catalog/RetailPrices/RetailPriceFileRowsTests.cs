using BuildingBlocks.Domain.Identifiers;
using Catalog.Application.Abstractions.Files;
using Catalog.Application.Features.RetailPrices.Commands.ImportRetailPrices;

namespace UnitTests.Catalog.RetailPrices;

/// <summary>Spec catalog/010 RP-02, RP-03: which rows of the file become prices and which are reported.</summary>
public sealed class RetailPriceFileRowsTests
{
    [Fact]
    public void Codes_AreTheNumbersOfRowsWithACode()
    {
        var codes = RetailPriceFileRows.Codes(
            [new RetailPriceSheetRow("561", 201m), new RetailPriceSheetRow(" 8202 ", 341m),
                new RetailPriceSheetRow("", null), new RetailPriceSheetRow("АБВ", 5m)]);

        Assert.Equal([561, 8202], codes);
    }

    [Fact]
    public void Plan_KeepsKnownProducts_ReportsUnknownAndPricelessCodes_LastRowWins()
    {
        RetailPriceSheetRow[] rows =
        [
            new("561", 201m),
            new("", null),
            new("8202", null),
            new("3066", 0m),
            new("999", 15m),
            new("АБВ", 5m),
            new("561", 205.5m)
        ];

        var plan = RetailPriceFileRows.Plan(rows, new HashSet<int> { 561, 8202, 3066 });

        Assert.Equal(205.5m, Assert.Single(plan.Prices, pair => pair.Key == ProductId.Create(561)).Value);
        Assert.Single(plan.Prices);
        Assert.Equal(["999", "АБВ"], plan.NotFoundCodes);
        Assert.Equal(["8202", "3066"], plan.WithoutPriceCodes);
    }
}
