using Content.Application.Features.Articles.Common;

namespace Content.Application.Features.Articles.Admin.Update;

public sealed class UpdateArticleCommandValidator : ArticleInputValidator<UpdateArticleCommand>
{
    public UpdateArticleCommandValidator() : base(slugRequired: true)
    {
        RuleFor(x => x.ArticleId).GreaterThan(0);
    }
}
