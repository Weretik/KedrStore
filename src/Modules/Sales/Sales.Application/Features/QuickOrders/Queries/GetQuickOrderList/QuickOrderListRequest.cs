namespace Sales.Application.Features.QuickOrders.Queries.GetQuickOrderList;

/// <summary>A page of site orders, newest first (spec sales/009, R-006).</summary>
public sealed record QuickOrderListRequest
{
    /// <summary>The only <see cref="Delivery"/> filter: not delivered yet with an error, or given up.</summary>
    public const string ProblemDelivery = "problem";

    public int Page { get; init; } = 1;
    public int PageSize { get; init; } = 20;

    /// <summary>Words of a name or phone; every word must match.</summary>
    public string? Search { get; init; }

    public string? Delivery { get; init; }
}
