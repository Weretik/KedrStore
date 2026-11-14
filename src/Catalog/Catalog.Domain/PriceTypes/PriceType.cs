using Catalog.Domain.Categories.Content;
using Catalog.Domain.Products.Prices;
using Catalog.Domain.Attributes;
using Catalog.Domain.Categories;
using Catalog.Domain.Groups;
using Catalog.Domain.PriceTypes;
using Catalog.Domain.Products;
using Catalog.Domain.Products.Media;
using Catalog.Domain.Slugs;

namespace Catalog.Domain.PriceTypes;

public class PriceType : BaseEntity<PriceTypeId>, IAggregateRoot
{
    public string PriceTypeName { get; private set; } = null!;

    private PriceType() { }
    private PriceType(PriceTypeId id, string priceTypeName)
    {
        Id = id;
        PriceTypeName = priceTypeName.Trim();
    }

    public static PriceType Create(PriceTypeId id, string priceTypeName)
        => new(id, priceTypeName);

    public void UpdateName(string priceTypeName)
        => PriceTypeName = priceTypeName.Trim();
}
