namespace Sales.Api.Endpoints.Orders;

/// <summary>The query of the order list; a missing page takes the list's default.</summary>
public sealed record GetAdminOrdersRequest
{
    [FromQuery(Name = "counterpartyId")]
    public string? CounterpartyId { get; init; }

    [FromQuery(Name = "page")]
    public int? Page { get; init; }

    [FromQuery(Name = "pageSize")]
    public int? PageSize { get; init; }
}
