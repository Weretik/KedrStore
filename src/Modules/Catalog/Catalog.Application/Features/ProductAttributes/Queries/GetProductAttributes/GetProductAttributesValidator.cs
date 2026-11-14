namespace Catalog.Application.Features.ProductAttributes.Queries.GetProductAttributes;

public sealed class GetProductAttributesValidator : AbstractValidator<GetProductAttributesQuery>
{
    public GetProductAttributesValidator()
    {
        RuleFor(query => query.ProductId).GreaterThan(0);
    }
}
