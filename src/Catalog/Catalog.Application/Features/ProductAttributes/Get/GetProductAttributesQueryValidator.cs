namespace Catalog.Application.Features.ProductAttributes.Get;

public sealed class GetProductAttributesQueryValidator : AbstractValidator<GetProductAttributesQuery>
{
    public GetProductAttributesQueryValidator()
    {
        RuleFor(query => query.ProductId).GreaterThan(0);
    }
}
