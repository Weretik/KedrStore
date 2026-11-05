using Sales.Application.Features.MobileSync.DTOs;

namespace Sales.Application.Features.MobileSync;

/// <summary>All active customers with their price rules for the offline manager app (spec platform/004, R-002).</summary>
public sealed record GetCustomersSnapshotQuery : IQuery<Result<CustomersSnapshotDto>>;
