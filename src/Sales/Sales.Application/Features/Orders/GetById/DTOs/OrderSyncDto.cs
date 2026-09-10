using Sales.Domain.Orders.Enums;

namespace Sales.Application.Features.Orders.GetById.DTOs;

public sealed record OrderSyncDto(
    OneCOrderSyncStatus Status,
    string? OneCDocumentNumber,
    DateTimeOffset? AcceptedAtUtc);
