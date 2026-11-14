namespace Catalog.Application.Features.Attributes.Queries.GetCatalogAttribute;

public sealed class GetCatalogAttributeQueryValidator : AbstractValidator<GetCatalogAttributeQuery>
{
    public GetCatalogAttributeQueryValidator()
    {
        RuleFor(query => query.AttributeId).GreaterThan(0);
    }
}
