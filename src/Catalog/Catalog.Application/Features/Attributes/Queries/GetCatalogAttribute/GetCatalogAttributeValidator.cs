namespace Catalog.Application.Features.Attributes.Queries.GetCatalogAttribute;

public sealed class GetCatalogAttributeValidator : AbstractValidator<GetCatalogAttributeQuery>
{
    public GetCatalogAttributeValidator()
    {
        RuleFor(query => query.AttributeId).GreaterThan(0);
    }
}
