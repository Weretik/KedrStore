using Content.Application.Features.Articles.Shared;
using Content.Application.Features.Articles.Shared.Writing;
using Content.Application.Features.Articles.Shared.Reading;

namespace Content.Application.Features.Articles.Commands.UpdateArticle;

public sealed record UpdateArticleCommand(long ArticleId, ArticleInput Input) : ICommand<Result<AdminArticleDetails>>, IArticleInputCommand;
