namespace Content.Application.Features.Articles.Queries.GetArticleCover;

public sealed class GetArticleCoverValidator : AbstractValidator<GetArticleCoverQuery>
{
    public GetArticleCoverValidator()
    {
        RuleFor(x => x.ArticleId).GreaterThan(0);
    }
}
