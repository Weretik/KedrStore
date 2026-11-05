using Catalog.Domain.ValueObjects;

namespace Catalog.Application.Features.Groups.GetForProduct;

public sealed class GetProductGroupForProductQueryValidator : AbstractValidator<GetProductGroupForProductQuery>
{
    public GetProductGroupForProductQueryValidator() => RuleFor(query => query.ProductId).GreaterThan(0);
}
