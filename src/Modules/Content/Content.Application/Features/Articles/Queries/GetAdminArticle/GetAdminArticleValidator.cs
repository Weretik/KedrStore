namespace Content.Application.Features.Articles.Queries.GetAdminArticle;

public sealed class GetAdminArticleValidator : AbstractValidator<GetAdminArticleQuery>
{
    public GetAdminArticleValidator()
    {
        RuleFor(x => x.ArticleId).GreaterThan(0);
    }
}
