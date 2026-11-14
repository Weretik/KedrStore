using BuildingBlocks.Domain.Abstractions.Errors;

namespace Catalog.Domain.Products.Media;

public static class ProductMediaErrors
{
    public static DomainError TooManyPhotos(int count, int maximum) =>
        new("Catalog.Product.Media.TooManyPhotos", $"Не більше {maximum} фото (зараз {count}).", Field: "PhotoFileIds");

    public static DomainError DuplicateFile() =>
        new("Catalog.Product.Media.DuplicateFile", "Один файл додано до товару двічі.", Field: "PhotoFileIds");
}
