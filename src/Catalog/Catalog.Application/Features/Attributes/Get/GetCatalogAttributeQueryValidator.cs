namespace Catalog.Application.Features.Attributes.Get;

public sealed class GetCatalogAttributeQueryValidator : AbstractValidator<GetCatalogAttributeQuery>
{
    public GetCatalogAttributeQueryValidator()
    {
        RuleFor(query => query.AttributeId).GreaterThan(0);
    }
}
