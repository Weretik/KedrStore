using Content.Application.Features.Articles.Shared;
using Content.Application.Features.Articles.Shared.Reading;

namespace Content.Application.Features.Articles.Queries.GetAdminArticle;

public sealed record GetAdminArticleQuery(long ArticleId) : IQuery<Result<AdminArticleDetails>>;
