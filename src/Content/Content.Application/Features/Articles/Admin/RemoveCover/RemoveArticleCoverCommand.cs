using Content.Application.Features.Articles.Common;

namespace Content.Application.Features.Articles.Admin.RemoveCover;

public sealed record RemoveArticleCoverCommand(long ArticleId) : ICommand<Result<AdminArticleDetails>>;
