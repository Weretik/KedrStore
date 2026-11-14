namespace Catalog.Application.Features.Categories.Queries.GetCategoryContent;

public sealed class GetCategoryContentValidator : AbstractValidator<GetCategoryContentQuery>
{
    public GetCategoryContentValidator() => RuleFor(x => x.CategoryId).GreaterThan(0);
}
