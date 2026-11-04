using Content.Application.Features.Articles.Common;

namespace Content.Application.Features.Articles.Admin.GetById;

public sealed record GetAdminArticleQuery(long ArticleId) : IQuery<Result<AdminArticleDetails>>;
