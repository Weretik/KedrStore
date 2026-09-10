using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc;

namespace Sales.Api.Contracts.Orders;

public sealed record GetAdminOrdersRequest
{
    [DisplayFormat(ConvertEmptyStringToNull = false)]
    [FromQuery(Name = "counterpartyId")]
    public string? CounterpartyId { get; init; }
    [FromQuery(Name = "page")]
    public int Page { get; init; } = 1;
    [FromQuery(Name = "pageSize")]
    public int PageSize { get; init; } = 20;
}
