namespace Catalog.Contracts.Overview;

/// <summary>
/// One page of the site in one language with what its search texts lack (spec platform/010). <c>Kind</c> is
/// <c>category</c> or <c>model</c>; <c>Issues</c> holds <c>noText</c>, <c>noDescription</c>, <c>longTitle</c>,
/// <c>longDescription</c>, <c>duplicateTitle</c>. Only pages with at least one issue are listed.
/// </summary>
public sealed record SeoReviewItemResponse(
    string Kind,
    long Id,
    string Name,
    string Language,
    IReadOnlyList<string> Issues);
