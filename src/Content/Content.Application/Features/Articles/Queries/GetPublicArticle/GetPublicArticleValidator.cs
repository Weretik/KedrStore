using Content.Application.Features.Articles.Shared;

namespace Content.Application.Features.Articles.Queries.GetPublicArticle;

public sealed class GetPublicArticleValidator : AbstractValidator<GetPublicArticleQuery>
{
    public GetPublicArticleValidator()
    {
        RuleFor(x => x.Language).Must(ArticleLanguage.IsSupported).WithMessage("Language must be 'uk' or 'ru'.");
        RuleFor(x => x.Slug).NotEmpty().MaximumLength(Article.SlugMaxLength);
    }
}
