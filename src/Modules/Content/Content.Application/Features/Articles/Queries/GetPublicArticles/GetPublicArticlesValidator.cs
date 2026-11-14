using Content.Application.Features.Articles.Shared;
using Content.Application.Features.Articles.Shared.Writing;

namespace Content.Application.Features.Articles.Queries.GetPublicArticles;

public sealed class GetPublicArticlesValidator : AbstractValidator<GetPublicArticlesQuery>
{
    public GetPublicArticlesValidator()
    {
        RuleFor(x => x.Language).Must(ArticleLanguage.IsSupported).WithMessage("Мова може бути лише uk або ru.");
    }
}
