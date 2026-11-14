using Content.Application.Features.Articles.Shared.Writing;
using Content.Application.Features.Articles.Shared;

namespace Content.Application.Features.Articles.Commands.UpdateArticle;

public sealed class UpdateArticleValidator : ArticleInputValidator<UpdateArticleCommand>
{
    public UpdateArticleValidator() : base(slugRequired: true)
    {
        RuleFor(x => x.ArticleId).GreaterThan(0);
    }
}
