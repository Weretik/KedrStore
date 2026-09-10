namespace Sales.Application.Features.Customers.GetList.DTOs;

public sealed record CustomerListRequest
{
    public int Page { get; init; } = 1;
    public int PageSize { get; init; } = 20;
}
