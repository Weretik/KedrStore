using BuildingBlocks.Domain.Abstractions.Errors;
using Catalog.Domain.Attributes.ProductValues;

namespace Catalog.Domain.Attributes;

public static class AttributeErrors
{
    public static DomainError Required(string field) =>
        new($"Catalog.Attribute.{field}.Required", "Заповніть це поле.", Field: field);

    public static DomainError TooLong(string field, int maxLength, int length) =>
        new($"Catalog.Attribute.{field}.TooLong", $"Не більше {maxLength} символів (зараз {length}).", Field: field);

    public static DomainError SlugInvalid() =>
        new("Catalog.Attribute.Slug.Invalid", "Лише a-z, 0-9 та одинарні дефіси.", Field: "Slug");

    public static DomainError SwatchHexInvalid() =>
        new("Catalog.AttributeValue.SwatchHex.Invalid", "Колір у форматі #RRGGBB.", Field: "SwatchHex");

    public static DomainError SwatchMediaInvalid() =>
        new("Catalog.AttributeValue.SwatchMedia.Invalid", "Оберіть зображення з медіатеки.", Field: "SwatchMediaFileId");

    public static DomainError ValueOrNumberRequired() =>
        new("Catalog.ProductAttribute.Value.Required", "Оберіть значення зі списку або введіть число.",
            Field: "ValueId");

    public static DomainError PositionInvalid() =>
        new("Catalog.ProductAttribute.Position.Invalid",
            $"Позиція має бути від 0 до {ProductAttributeValue.MaxPosition}.", Field: "Position");
}
