using Catalog.Domain.Entities.Attributes;

namespace Catalog.Application.Features.Attributes.Common;

/// <summary>Request checks shared by creating and editing a value.</summary>
internal static class AttributeValueRules
{
    public const string HexPattern = "^#[0-9A-Fa-f]{6}$";

    public static void NameAndSwatch<T>(AbstractValidator<T> validator, Func<T, string?> nameUk, Func<T, string?> nameRu,
        Func<T, int> sortOrder, Func<T, string?> swatchHex, Func<T, long?> swatchMedia, Func<T, bool> hasRequest)
    {
        validator.RuleFor(item => nameUk(item)).NotEmpty().MaximumLength(AttributeValue.NameMaxLength)
            .When(hasRequest).OverridePropertyName("nameUk");
        validator.RuleFor(item => nameRu(item)).MaximumLength(AttributeValue.NameMaxLength)
            .When(hasRequest).OverridePropertyName("nameRu");
        validator.RuleFor(item => sortOrder(item)).InclusiveBetween(0, 100_000)
            .When(hasRequest).OverridePropertyName("sortOrder");
        validator.RuleFor(item => swatchHex(item)).Matches(HexPattern).WithMessage("Swatch color must be #RRGGBB.")
            .When(item => hasRequest(item) && !string.IsNullOrWhiteSpace(swatchHex(item)))
            .OverridePropertyName("swatchHex");
        validator.RuleFor(item => swatchMedia(item)).GreaterThan(0)
            .When(item => hasRequest(item) && swatchMedia(item) is not null).OverridePropertyName("swatchMediaFileId");
    }
}
