using Content.Application.Features.Articles.Shared.Writing;
using Content.Application.Features.Articles.Shared;

namespace Content.Application.Features.Articles.Commands.CreateArticle;

public sealed class CreateArticleValidator() : ArticleInputValidator<CreateArticleCommand>(slugRequired: false);
