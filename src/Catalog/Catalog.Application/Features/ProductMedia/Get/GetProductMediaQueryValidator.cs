namespace Catalog.Application.Features.ProductMedia.Get;

public sealed class GetProductMediaQueryValidator : AbstractValidator<GetProductMediaQuery>
{
    public GetProductMediaQueryValidator() => RuleFor(query => query.ProductId).GreaterThan(0);
}
