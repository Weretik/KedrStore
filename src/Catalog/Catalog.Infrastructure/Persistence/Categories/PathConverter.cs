using Catalog.Infrastructure.Integrations.OneC;
using Catalog.Domain.Attributes;
using Catalog.Domain.Categories;
using Catalog.Domain.Groups;
using Catalog.Domain.PriceTypes;
using Catalog.Domain.Products;
using Catalog.Domain.Products.Media;
using Catalog.Domain.Slugs;

namespace Catalog.Infrastructure.Persistence.Categories;

public static class PathConverter
{
    public static readonly ValueConverter<CategoryPath, LTree> Convert =
    new(
        path => (LTree)path.Value,
        value => CategoryPath.From(value)
    );
}
