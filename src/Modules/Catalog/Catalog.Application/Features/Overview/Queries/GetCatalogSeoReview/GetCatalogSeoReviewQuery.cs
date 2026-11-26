using Catalog.Contracts.Overview;

namespace Catalog.Application.Features.Overview.Queries.GetCatalogSeoReview;

/// <summary>Categories and accepted models open to search engines whose search texts need work.</summary>
public sealed record GetCatalogSeoReviewQuery : IQuery<Result<IReadOnlyList<SeoReviewItemResponse>>>;
