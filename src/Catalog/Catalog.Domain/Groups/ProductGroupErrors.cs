using Catalog.Domain.Attributes;
using Catalog.Domain.Categories;
using Catalog.Domain.Categories.Content;
using Catalog.Domain.Products;
using Catalog.Domain.Products.Media;
using Catalog.Domain.Products.Prices;
namespace Catalog.Domain.Groups;

public static class ProductGroupErrors
{
    public static CatalogDomainError Required(string field) =>
        new($"Catalog.ProductGroup.{field}.Required", $"{field} is required");

    public static CatalogDomainError TooLong(string field, int maxLength, int length) =>
        new($"Catalog.ProductGroup.{field}.TooLong", $"{field} must be at most {maxLength} characters. Actual: {length}");
}
