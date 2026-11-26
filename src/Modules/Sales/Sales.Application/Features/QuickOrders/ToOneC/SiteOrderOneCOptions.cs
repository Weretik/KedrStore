namespace Sales.Application.Features.QuickOrders.ToOneC;

/// <summary>Who site orders are placed on in 1C (spec sales/009, R-010).</summary>
public sealed class SiteOrderOneCOptions
{
    public const string SectionName = "Sales:SiteOrders";

    /// <summary>The 1C counterparty of every site order: «РОЗНИЦА ХАЛТУРИНА».</summary>
    public string CounterpartyId { get; init; } = "000001588";
}
