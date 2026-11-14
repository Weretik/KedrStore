using Content.Application.Features.Articles.Shared;

namespace Content.Application.Features.Articles.Commands.UpdateArticle;

public sealed record UpdateArticleCommand(long ArticleId, ArticleInput Input) : ICommand<Result<AdminArticleDetails>>, IArticleInputCommand;
