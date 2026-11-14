namespace Catalog.Application.Features.ProductMedia.Queries.GetProductMedia;

public sealed class GetProductMediaQueryValidator : AbstractValidator<GetProductMediaQuery>
{
    public GetProductMediaQueryValidator() => RuleFor(query => query.ProductId).GreaterThan(0);
}
