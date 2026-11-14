using Catalog.Domain.Attributes;
using Catalog.Domain.Categories;
using Catalog.Domain.Groups;
using Catalog.Domain.PriceTypes;
using Catalog.Domain.Products;
using Catalog.Domain.Products.Media;
using Catalog.Domain.Slugs;

namespace Catalog.Infrastructure.Configurations.Attributes;

internal static class AttributeIdConverters
{
    public static readonly ValueConverter<AttributeId, int> AttributeId =
        new(id => id.Value, value => Domain.Attributes.AttributeId.FromStorage(value));

    public static readonly ValueConverter<AttributeValueId, int> ValueId =
        new(id => id.Value, value => AttributeValueId.FromStorage(value));

    public static readonly ValueConverter<AttributeAliasId, int> AliasId =
        new(id => id.Value, value => AttributeAliasId.FromStorage(value));
}
