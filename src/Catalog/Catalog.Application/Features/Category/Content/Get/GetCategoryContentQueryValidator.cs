namespace Catalog.Application.Features.Category.Content.Get;

public sealed class GetCategoryContentQueryValidator : AbstractValidator<GetCategoryContentQuery>
{
    public GetCategoryContentQueryValidator() => RuleFor(x => x.CategoryId).GreaterThan(0);
}
