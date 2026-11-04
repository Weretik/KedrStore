namespace Content.Application.Features.Articles.Public.GetCover;

public sealed class GetArticleCoverQueryValidator : AbstractValidator<GetArticleCoverQuery>
{
    public GetArticleCoverQueryValidator()
    {
        RuleFor(x => x.ArticleId).GreaterThan(0);
    }
}
