using Catalog.Domain.Attributes;
using Catalog.Domain.Groups;
using Catalog.Domain.Categories;
using Catalog.Domain.Categories.Content;
using Catalog.Domain.PriceTypes;
using Catalog.Domain.Products;
using Catalog.Domain.Products.Media;
using Catalog.Domain.Products.Prices;
using Catalog.Domain.Slugs;

namespace Catalog.Domain.Products.Media;

public static class ProductMediaErrors
{
    public static CatalogDomainError TooManyPhotos(int count, int maximum) =>
        new("Catalog.Product.Media.TooManyPhotos",
            $"A product can have at most {maximum} photos. Actual: {count}");

    public static CatalogDomainError DuplicateFile(long fileId) =>
        new("Catalog.Product.Media.DuplicateFile",
            $"Media file {fileId} is linked to the product more than once");

    public static CatalogDomainError FileIdInvalid(long fileId) =>
        new("Catalog.Product.Media.FileIdInvalid",
            $"Media file id must be positive. Actual: {fileId}");

    public static CatalogDomainError UrlInvalid() =>
        new("Catalog.Product.Media.UrlInvalid",
            $"Media file URL is required and must be at most {ProductMediaLink.UrlMaxLength} characters");
}
