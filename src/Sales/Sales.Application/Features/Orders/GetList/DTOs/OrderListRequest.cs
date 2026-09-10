namespace Sales.Application.Features.Orders.GetList.DTOs;

public sealed record OrderListRequest
{
    public string? CounterpartyId { get; init; }
    public int Page { get; init; } = 1;
    public int PageSize { get; init; } = 20;
}
