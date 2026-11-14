using Catalog.Application.Shared;

namespace Catalog.Application.Features.Categories.Queries.GetCategoryBySlug;

public sealed class GetCategoryBySlugValidator : AbstractValidator<GetCategoryBySlugQuery>
{
    public GetCategoryBySlugValidator()
    {
        RuleFor(x => x.Lang).SupportedLanguage();
        RuleFor(x => x.Slug).LookupSlug();
    }
}
