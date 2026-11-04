using Content.Application.Features.Articles.Common;

namespace Content.Application.Features.Articles.Public.GetBySlug;

/// <summary>One published article by its slug; unpublished and missing articles are not found.</summary>
public sealed record GetPublicArticleQuery(string Language, string Slug) : IQuery<Result<PublicArticleDetails>>;
