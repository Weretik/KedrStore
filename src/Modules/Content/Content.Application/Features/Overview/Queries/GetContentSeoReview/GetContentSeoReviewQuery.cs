namespace Content.Application.Features.Overview.Queries.GetContentSeoReview;

/// <summary>Published articles whose search texts need work, per language (spec platform/010).</summary>
public sealed record GetContentSeoReviewQuery : IQuery<Result<IReadOnlyList<ContentSeoReviewItem>>>;
