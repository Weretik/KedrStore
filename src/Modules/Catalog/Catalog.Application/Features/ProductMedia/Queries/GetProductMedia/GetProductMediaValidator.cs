namespace Catalog.Application.Features.ProductMedia.Queries.GetProductMedia;

public sealed class GetProductMediaValidator : AbstractValidator<GetProductMediaQuery>
{
    public GetProductMediaValidator() => RuleFor(query => query.ProductId).GreaterThan(0);
}
