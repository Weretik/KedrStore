using System.ComponentModel;

namespace Catalog.Application.Features.Categories.Shared.Tree;

/// <summary>
/// The cached public tree. Sealed and immutable, so the in-memory cache hands out this instance instead of copying it
/// on every hit (HybridCache rule).
/// </summary>
[ImmutableObject(true)]
public sealed record CategoryTreeSnapshot(IReadOnlyList<CategoryTreeDto> Roots);
