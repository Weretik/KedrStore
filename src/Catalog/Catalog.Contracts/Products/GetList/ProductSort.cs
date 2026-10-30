namespace Catalog.Contracts.Products.GetList;

public enum ProductSort
{
    IdAsc,
    IdDesc,
    NameAsc,
    NameDesc,
    PriceAsc,
    PriceDesc,
    CategoryAsc,
    CategoryDesc,
    /// <summary>In stock first, then name in the requested language, then ID.</summary>
    AvailabilityThenName,
}
