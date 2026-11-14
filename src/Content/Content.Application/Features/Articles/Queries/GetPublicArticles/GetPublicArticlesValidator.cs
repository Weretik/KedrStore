using Content.Application.Features.Articles.Shared;

namespace Content.Application.Features.Articles.Queries.GetPublicArticles;

public sealed class GetPublicArticlesValidator : AbstractValidator<GetPublicArticlesQuery>
{
    public GetPublicArticlesValidator()
    {
        RuleFor(x => x.Language).Must(ArticleLanguage.IsSupported).WithMessage("Language must be 'uk' or 'ru'.");
    }
}
