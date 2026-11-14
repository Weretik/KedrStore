namespace Catalog.Domain.Slugs;

/// <summary>Catalog entities that own a public URL slug.</summary>
public enum CatalogEntityType : short
{
    Product = 1,
    Category = 2,
    ProductGroup = 3
}
