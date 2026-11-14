using Catalog.Application.Features.Categories.Shared;
namespace Catalog.Application.Features.Categories.Queries.GetCategoryBySlug;

public sealed class GetCategoryBySlugValidator : AbstractValidator<GetCategoryBySlugQuery>
{
    public GetCategoryBySlugValidator()
    {
        RuleFor(x => x.Lang).Must(x => x is "uk" or "ru");
        RuleFor(x => x.Slug).NotEmpty().MaximumLength(100);
    }
}
