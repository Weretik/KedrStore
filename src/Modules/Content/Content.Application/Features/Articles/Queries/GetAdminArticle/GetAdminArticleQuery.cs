using Content.Application.Features.Articles.Shared;

namespace Content.Application.Features.Articles.Queries.GetAdminArticle;

public sealed record GetAdminArticleQuery(long ArticleId) : IQuery<Result<AdminArticleDetails>>;
