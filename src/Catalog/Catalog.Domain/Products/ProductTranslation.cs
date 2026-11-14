namespace Catalog.Domain.Products;

public class ProductTranslation : BaseAuditableEntity<int>, IAggregateRoot
{
    public ProductId ProductId { get; private set; }
    public string Language { get; private set; } = null!;
    public string Name { get; private set; } = null!;

    private ProductTranslation() { }

    public const int NameMaxLength = 300;

    public static DomainResult<ProductTranslation> Create(ProductId productId, string language, string name,
        DateTimeOffset createdAt)
    {
        var checks = new DomainChecks();
        if (productId.Value <= 0)
            checks.Check(ProductTranslationErrors.ProductIdRequired());
        var checkedLanguage = checks.Take(CheckLanguage(language));
        var checkedName = checks.Take(CheckName(name));
        if (checks.Error is { } error)
            return error;

        var translation = new ProductTranslation { ProductId = productId, Language = checkedLanguage, Name = checkedName };
        translation.MarkAsCreated(createdAt);
        return translation;
    }

    public DomainResult Update(string name, DateTimeOffset updatedAt)
    {
        var checkedName = CheckName(name);
        if (checkedName.IsFailure)
            return checkedName;
        Name = checkedName.Value;
        MarkAsUpdated(updatedAt);
        return DomainResult.Success();
    }

    /// <summary>Sets the name and revives a soft-deleted translation. Returns whether anything changed.</summary>
    public DomainResult<bool> Apply(string name, DateTimeOffset now)
    {
        if (!IsDeleted && string.Equals(Name, name?.Trim(), StringComparison.Ordinal))
            return false;

        var checkedName = CheckName(name!);
        if (checkedName.IsFailure)
            return checkedName.Error!;
        Name = checkedName.Value;
        IsDeleted = false;
        MarkAsUpdated(now);
        return true;
    }

    private static DomainResult<string> CheckLanguage(string language)
    {
        if (string.IsNullOrWhiteSpace(language))
            return ProductTranslationErrors.LanguageRequired();

        var trimmed = language.Trim().ToLowerInvariant();
        return trimmed.Length is < 2 or > 10 ? ProductTranslationErrors.LanguageInvalid() : trimmed;
    }

    private static DomainResult<string> CheckName(string name)
    {
        if (string.IsNullOrWhiteSpace(name))
            return ProductTranslationErrors.NameRequired();

        var trimmed = name.Trim();
        return trimmed.Length > NameMaxLength ? ProductTranslationErrors.NameTooLong(trimmed.Length) : trimmed;
    }
}
