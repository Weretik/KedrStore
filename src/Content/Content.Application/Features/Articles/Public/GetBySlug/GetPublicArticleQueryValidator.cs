using Content.Application.Features.Articles.Common;

namespace Content.Application.Features.Articles.Public.GetBySlug;

public sealed class GetPublicArticleQueryValidator : AbstractValidator<GetPublicArticleQuery>
{
    public GetPublicArticleQueryValidator()
    {
        RuleFor(x => x.Language).Must(ArticleLanguage.IsSupported).WithMessage("Language must be 'uk' or 'ru'.");
        RuleFor(x => x.Slug).NotEmpty().MaximumLength(Article.SlugMaxLength);
    }
}
