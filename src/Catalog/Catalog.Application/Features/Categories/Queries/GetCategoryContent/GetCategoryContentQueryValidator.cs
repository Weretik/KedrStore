using Catalog.Application.Features.Categories.Shared;
namespace Catalog.Application.Features.Categories.Queries.GetCategoryContent;

public sealed class GetCategoryContentQueryValidator : AbstractValidator<GetCategoryContentQuery>
{
    public GetCategoryContentQueryValidator() => RuleFor(x => x.CategoryId).GreaterThan(0);
}
