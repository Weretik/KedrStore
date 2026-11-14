using Catalog.Contracts.Slugs;
using Catalog.Domain.Attributes;
using Catalog.Domain.Categories;
using Catalog.Domain.Groups;
using Catalog.Domain.PriceTypes;
using Catalog.Domain.Products;
using Catalog.Domain.Products.Media;
using Catalog.Domain.Slugs;

namespace Catalog.Application.Features.Slugs.Commands.ChangeSlug;

public sealed record ChangeSlugCommand(CatalogEntityType EntityType, int Id, ChangeSlugRequest Request)
    : ICommand<Result<ChangeSlugResponse>>
{
    public static ChangeSlugCommand ForProduct(int id, ChangeSlugRequest request) => new(CatalogEntityType.Product, id, request);
    public static ChangeSlugCommand ForCategory(int id, ChangeSlugRequest request) => new(CatalogEntityType.Category, id, request);
    public static ChangeSlugCommand ForProductGroup(int id, ChangeSlugRequest request) => new(CatalogEntityType.ProductGroup, id, request);
}
