namespace Sales.Application.Features.Customers.GetList.DTOs;

public sealed record CustomerListItemDto(
    string CounterpartyId,
    string Name,
    string? Phone);
