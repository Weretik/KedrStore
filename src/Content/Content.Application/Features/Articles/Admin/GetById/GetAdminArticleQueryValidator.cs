namespace Content.Application.Features.Articles.Admin.GetById;

public sealed class GetAdminArticleQueryValidator : AbstractValidator<GetAdminArticleQuery>
{
    public GetAdminArticleQueryValidator()
    {
        RuleFor(x => x.ArticleId).GreaterThan(0);
    }
}
