namespace Sales.Application.Features.Customers.GetList.DTOs;

public sealed record CustomerListRequest
{
    public int Page { get; init; } = 1;
    public int PageSize { get; init; } = 20;
    public string? SearchTerm { get; init; }
    public string? CounterpartyId { get; init; }
    public string? Name { get; init; }
    public string? Phone { get; init; }
    public string Sort { get; init; } = "name";
}
