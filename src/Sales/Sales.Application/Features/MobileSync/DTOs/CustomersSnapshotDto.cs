namespace Sales.Application.Features.MobileSync.DTOs;

/// <summary>
/// All active customers for the offline manager app (spec platform/004, R-002). Debt and balance stay <c>null</c>
/// until 1C sends settlements.
/// </summary>
/// <param name="Version">Hash of the content; the same value is sent as the <c>ETag</c>.</param>
/// <param name="GeneratedAt">When the server built this answer; not part of the version.</param>
public sealed record CustomersSnapshotDto(
    string Version,
    DateTimeOffset GeneratedAt,
    IReadOnlyList<CustomerSnapshotDto> Customers);

/// <param name="CategoryPriceTypes">
/// Price rules by category. The nearest rule on a product's category or any of its ancestors wins, otherwise
/// <paramref name="DefaultPriceTypeId"/>, as the order workspace prices lines.
/// </param>
public sealed record CustomerSnapshotDto(
    string CounterpartyId,
    string Name,
    string? Phone,
    string Email,
    int DefaultPriceTypeId,
    IReadOnlyList<CustomerSnapshotPriceRuleDto> CategoryPriceTypes,
    decimal? Debt,
    decimal? Balance);

public sealed record CustomerSnapshotPriceRuleDto(int CategoryId, int PriceTypeId);
