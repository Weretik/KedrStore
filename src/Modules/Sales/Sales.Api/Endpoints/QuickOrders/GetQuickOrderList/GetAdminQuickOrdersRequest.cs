namespace Sales.Api.Endpoints.QuickOrders.GetQuickOrderList;

/// <summary>The query of the site order list; a missing value takes the list's default.</summary>
public sealed record GetAdminQuickOrdersRequest
{
    [FromQuery(Name = "page")]
    public int? Page { get; init; }

    [FromQuery(Name = "pageSize")]
    public int? PageSize { get; init; }

    /// <summary>Words of a buyer's name or phone.</summary>
    [FromQuery(Name = "search")]
    public string? Search { get; init; }

    /// <summary><c>problem</c>: only orders Telegram has not got and that had an error or were given up.</summary>
    [FromQuery(Name = "delivery")]
    public string? Delivery { get; init; }
}
