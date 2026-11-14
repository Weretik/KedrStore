using Sales.Application.Features.MobileSync.Queries.GetCustomersSnapshot;

namespace Sales.Application.Features.MobileSync.Queries.GetCustomersSnapshot;

/// <summary>All active customers with their price rules for the offline manager app (spec platform/004, R-002).</summary>
public sealed record GetCustomersSnapshotQuery : IQuery<Result<CustomersSnapshotDto>>;
