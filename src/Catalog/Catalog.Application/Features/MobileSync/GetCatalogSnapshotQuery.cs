using Catalog.Contracts.MobileSync;

namespace Catalog.Application.Features.MobileSync;

/// <summary>The whole sales catalog for the offline manager app (spec platform/004, R-001).</summary>
public sealed record GetCatalogSnapshotQuery : IQuery<Result<CatalogSnapshotResponse>>;
