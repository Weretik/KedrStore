using Catalog.Domain.Attributes;
using Catalog.Domain.Categories;
using Catalog.Domain.Groups;
using Catalog.Domain.PriceTypes;
using Catalog.Domain.Products;
using Catalog.Domain.Products.Media;
using Catalog.Domain.Slugs;

namespace Catalog.Application.Features.Groups.Queries.GetProductGroupForProduct;

public sealed class GetProductGroupForProductQueryValidator : AbstractValidator<GetProductGroupForProductQuery>
{
    public GetProductGroupForProductQueryValidator() => RuleFor(query => query.ProductId).GreaterThan(0);
}
