using Content.Application.Features.Articles.Shared;
using Content.Application.Features.Articles.Shared.Writing;

namespace Content.Application.Features.Articles.Queries.GetPublicArticle;

public sealed class GetPublicArticleValidator : AbstractValidator<GetPublicArticleQuery>
{
    public GetPublicArticleValidator()
    {
        RuleFor(x => x.Language).Must(ArticleLanguage.IsSupported).WithMessage("Мова може бути лише uk або ru.");
        RuleFor(x => x.Slug).NotEmpty().MaximumLength(Article.SlugMaxLength);
    }
}
