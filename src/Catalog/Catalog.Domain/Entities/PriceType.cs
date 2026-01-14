using Catalog.Domain.ValueObjects;

namespace Catalog.Domain.Entities;

public class PriceType : BaseEntity<PriceTypeId>, IAggregateRoot
{
    public string PriceTypeName { get; private set; }

    private PriceType() { }
    private PriceType(PriceTypeId id, string priceTypeName, string productTypeIdOneC)
    {
        Id = id;
        PriceTypeName = priceTypeName.Trim();
    }

    public static PriceType Create(PriceTypeId id, string priceTypeName, string productTypeIdOneC)
        => new(id, priceTypeName, productTypeIdOneC);

    public void UpdateName(string priceTypeName)
        => PriceTypeName = priceTypeName.Trim();
}
