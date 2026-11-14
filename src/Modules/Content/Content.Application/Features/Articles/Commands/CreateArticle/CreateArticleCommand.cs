using Content.Application.Features.Articles.Shared;
using Content.Application.Features.Articles.Shared.Writing;
using Content.Application.Features.Articles.Shared.Reading;

namespace Content.Application.Features.Articles.Commands.CreateArticle;

public sealed record CreateArticleCommand(ArticleInput Input) : ICommand<Result<AdminArticleDetails>>, IArticleInputCommand;
