using Catalog.Domain.Errors;
using Catalog.Domain.ValueObjects;

namespace Catalog.Domain.Entities;

public class ProductTranslation : BaseAuditableEntity<int>, IAggregateRoot
{
    public ProductId ProductId { get; private set; }
    public string Language { get; private set; } = null!;
    public string Name { get; private set; } = null!;

    private ProductTranslation() { }

    private ProductTranslation(ProductId productId, string language, string name, DateTimeOffset createdAt)
    {
        SetProductId(productId);
        SetLanguage(language);
        SetName(name);
        MarkAsCreated(createdAt);
    }

    public static ProductTranslation Create(ProductId productId, string language, string name, DateTimeOffset createdAt)
        => new(productId, language, name, createdAt);

    public void Update(string name, DateTimeOffset updatedAt)
    {
        SetName(name);
        MarkAsUpdated(updatedAt);
    }

    private void SetProductId(ProductId productId)
    {
        if (productId.Value <= 0)
            throw new DomainException(ProductTranslationErrors.ProductIdRequired());

        ProductId = productId;
    }

    private void SetLanguage(string language)
    {
        if (string.IsNullOrWhiteSpace(language))
            throw new DomainException(ProductTranslationErrors.LanguageRequired());

        var trimmed = language.Trim().ToLowerInvariant();

        if (trimmed.Length is < 2 or > 10)
            throw new DomainException(ProductTranslationErrors.LanguageLengthInvalid(trimmed.Length));

        Language = trimmed;
    }

    private void SetName(string name)
    {
        if (string.IsNullOrWhiteSpace(name))
            throw new DomainException(ProductTranslationErrors.NameRequired());

        var trimmed = name.Trim();

        if (trimmed.Length is < 1 or > 300)
            throw new DomainException(ProductTranslationErrors.NameLengthInvalid(trimmed.Length));

        Name = trimmed;
    }
}
