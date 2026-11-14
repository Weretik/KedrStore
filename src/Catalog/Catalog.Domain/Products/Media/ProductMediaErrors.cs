namespace Catalog.Domain.Products.Media;

public static class ProductMediaErrors
{
    public static DomainError TooManyPhotos(int count, int maximum) =>
        new("Catalog.Product.Media.TooManyPhotos",
            $"A product can have at most {maximum} photos. Actual: {count}");

    public static DomainError DuplicateFile(long fileId) =>
        new("Catalog.Product.Media.DuplicateFile",
            $"Media file {fileId} is linked to the product more than once");

    public static DomainError FileIdInvalid(long fileId) =>
        new("Catalog.Product.Media.FileIdInvalid",
            $"Media file id must be positive. Actual: {fileId}");

    public static DomainError UrlInvalid() =>
        new("Catalog.Product.Media.UrlInvalid",
            $"Media file URL is required and must be at most {ProductMediaLink.UrlMaxLength} characters");
}
