using Catalog.Application.Features.Categories.Shared;
namespace Catalog.Application.Features.Categories.Queries.GetCategoryBySlug;

public sealed class GetCategoryBySlugQueryValidator : AbstractValidator<GetCategoryBySlugQuery>
{
    public GetCategoryBySlugQueryValidator()
    {
        RuleFor(x => x.Lang).Must(x => x is "uk" or "ru");
        RuleFor(x => x.Slug).NotEmpty().MaximumLength(100);
    }
}
