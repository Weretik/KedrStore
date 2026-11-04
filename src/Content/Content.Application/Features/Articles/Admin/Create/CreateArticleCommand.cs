using Content.Application.Features.Articles.Common;

namespace Content.Application.Features.Articles.Admin.Create;

public sealed record CreateArticleCommand(ArticleInput Input) : ICommand<Result<AdminArticleDetails>>, IArticleInputCommand;
