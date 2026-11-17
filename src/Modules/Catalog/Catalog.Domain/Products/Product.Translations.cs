namespace Catalog.Domain.Products;

/// <summary>
/// Product names in other languages: at most one per language, soft-deleted and revived instead of duplicated.
/// The 1C import changes them only through these methods; reads query the translation table directly.
/// </summary>
public partial class Product
{
    private readonly List<ProductTranslation> _translations = [];

    /// <summary>Loaded only by the commands that change translations, including soft-deleted ones.</summary>
    public IReadOnlyList<ProductTranslation> Translations => _translations;

    /// <summary>Creates, revives or renames the translation of <paramref name="language"/>.</summary>
    public DomainResult<TranslationChange> SetTranslation(string language, string name, DateTimeOffset now)
    {
        var existing = FindTranslation(language);
        if (existing is not null)
        {
            var applied = existing.Apply(name, now);
            if (applied.IsFailure)
                return applied.Error!;
            return applied.Value ? TranslationChange.Updated : TranslationChange.Unchanged;
        }

        var created = ProductTranslation.Create(Id, language, name, now);
        if (created.IsFailure)
            return created.Error!;
        _translations.Add(created.Value);
        return TranslationChange.Created;
    }

    /// <summary>Soft-deletes the translation of <paramref name="language"/>; returns whether one was removed.</summary>
    public bool RemoveTranslation(string language, DateTimeOffset now)
    {
        if (FindTranslation(language) is not { IsDeleted: false } existing)
            return false;
        existing.MarkAsDeleted(now);
        return true;
    }

    private ProductTranslation? FindTranslation(string language)
    {
        var normalized = language?.Trim().ToLowerInvariant();
        return _translations.FirstOrDefault(translation => translation.Language == normalized);
    }
}

public enum TranslationChange
{
    Unchanged,
    Created,
    Updated,
}
