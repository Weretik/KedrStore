namespace Catalog.Application.Features.ProductAttributes.Queries.GetProductAttributes;

public sealed class GetProductAttributesQueryValidator : AbstractValidator<GetProductAttributesQuery>
{
    public GetProductAttributesQueryValidator()
    {
        RuleFor(query => query.ProductId).GreaterThan(0);
    }
}
