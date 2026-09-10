using Microsoft.AspNetCore.Mvc;

namespace Sales.Api.Contracts.Customers;

public sealed record GetAdminCustomersRequest
{
    [FromQuery(Name = "page")]
    public int Page { get; init; } = 1;

    [FromQuery(Name = "pageSize")]
    public int PageSize { get; init; } = 20;
}
