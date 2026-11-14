using Catalog.Domain.Attributes;
using Catalog.Domain.Categories;
using Catalog.Domain.Groups;
using Catalog.Domain.PriceTypes;
using Catalog.Domain.Products;
using Catalog.Domain.Products.Media;
namespace Catalog.Domain.Slugs;

/// <summary>Catalog entities that own a public URL slug.</summary>
public enum CatalogEntityType : short
{
    Product = 1,
    Category = 2,
    ProductGroup = 3
}
