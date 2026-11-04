namespace Content.Application.Features.Articles.Admin.RemoveCover;

public sealed class RemoveArticleCoverCommandValidator : AbstractValidator<RemoveArticleCoverCommand>
{
    public RemoveArticleCoverCommandValidator()
    {
        RuleFor(x => x.ArticleId).GreaterThan(0);
    }
}
