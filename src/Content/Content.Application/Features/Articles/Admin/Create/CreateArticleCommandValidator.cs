using Content.Application.Features.Articles.Common;

namespace Content.Application.Features.Articles.Admin.Create;

public sealed class CreateArticleCommandValidator() : ArticleInputValidator<CreateArticleCommand>(slugRequired: false);
