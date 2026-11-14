namespace Sales.Api.Endpoints.Customers;

/// <summary>The query of the customer list; a missing value takes the list's default.</summary>
public sealed record GetAdminCustomersRequest
{
    [FromQuery(Name = "page")]
    public int? Page { get; init; }

    [FromQuery(Name = "pageSize")]
    public int? PageSize { get; init; }

    [FromQuery(Name = "searchTerm")]
    public string? SearchTerm { get; init; }

    [FromQuery(Name = "counterpartyId")]
    public string? CounterpartyId { get; init; }

    [FromQuery(Name = "name")]
    public string? Name { get; init; }

    [FromQuery(Name = "phone")]
    public string? Phone { get; init; }

    [FromQuery(Name = "sort")]
    public string? Sort { get; init; }
}
