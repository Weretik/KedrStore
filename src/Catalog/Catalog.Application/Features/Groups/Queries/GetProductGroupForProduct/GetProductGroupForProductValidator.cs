namespace Catalog.Application.Features.Groups.Queries.GetProductGroupForProduct;

public sealed class GetProductGroupForProductValidator : AbstractValidator<GetProductGroupForProductQuery>
{
    public GetProductGroupForProductValidator() => RuleFor(query => query.ProductId).GreaterThan(0);
}
