using Content.Application.Features.Articles.Shared;

namespace Content.Application.Features.Articles.Commands.CreateArticle;

public sealed record CreateArticleCommand(ArticleInput Input) : ICommand<Result<AdminArticleDetails>>, IArticleInputCommand;
