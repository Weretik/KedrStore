using Content.Application.Features.Articles.Common;

namespace Content.Application.Features.Articles.Admin.Update;

public sealed record UpdateArticleCommand(long ArticleId, ArticleInput Input) : ICommand<Result<AdminArticleDetails>>, IArticleInputCommand;
