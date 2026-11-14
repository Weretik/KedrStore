using Content.Application.Features.Articles.Shared;

namespace Content.Application.Features.Articles.Queries.GetPublicArticle;

/// <summary>One published article by its slug; unpublished and missing articles are not found.</summary>
public sealed record GetPublicArticleQuery(string Language, string Slug) : IQuery<Result<PublicArticleDetails>>;
