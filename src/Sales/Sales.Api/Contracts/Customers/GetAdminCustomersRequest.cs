using Microsoft.AspNetCore.Mvc;

namespace Sales.Api.Contracts.Customers;

public sealed record GetAdminCustomersRequest
{
    [FromQuery(Name = "page")]
    public int Page { get; init; } = 1;

    [FromQuery(Name = "pageSize")]
    public int PageSize { get; init; } = 20;

    [FromQuery(Name = "searchTerm")]
    public string? SearchTerm { get; init; }

    [FromQuery(Name = "counterpartyId")]
    public string? CounterpartyId { get; init; }

    [FromQuery(Name = "name")]
    public string? Name { get; init; }

    [FromQuery(Name = "phone")]
    public string? Phone { get; init; }

    [FromQuery(Name = "sort")]
    public string Sort { get; init; } = "name";
}
