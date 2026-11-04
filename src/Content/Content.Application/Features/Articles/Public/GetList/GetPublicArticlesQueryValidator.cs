using Content.Application.Features.Articles.Common;

namespace Content.Application.Features.Articles.Public.GetList;

public sealed class GetPublicArticlesQueryValidator : AbstractValidator<GetPublicArticlesQuery>
{
    public GetPublicArticlesQueryValidator()
    {
        RuleFor(x => x.Language).Must(ArticleLanguage.IsSupported).WithMessage("Language must be 'uk' or 'ru'.");
    }
}
