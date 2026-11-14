namespace Catalog.Domain.Groups;

/// <summary>Description and SEO texts of a model in one language (spec catalog/007): one text for all its products.</summary>
public class ProductGroupText : IAggregateRoot
{
    public static readonly IReadOnlyList<string> Languages = ["uk", "ru"];
    public const int ShortDescriptionMaxLength = 500;
    public const int DescriptionMaxLength = 8000;
    public const int SeoTitleMaxLength = 120;
    public const int SeoDescriptionMaxLength = 320;

    public ProductGroupId GroupId { get; private set; }
    public string Language { get; private set; } = null!;
    public string ShortDescription { get; private set; } = string.Empty;
    public string Description { get; private set; } = string.Empty;
    public string SeoTitle { get; private set; } = string.Empty;
    public string SeoDescription { get; private set; } = string.Empty;
    public DateTimeOffset UpdatedAt { get; private set; }

    /// <summary>The texts «Огляд» asks for: a short description and an SEO description.</summary>
    public bool IsComplete => ShortDescription.Length > 0 && SeoDescription.Length > 0;

    private ProductGroupText() { }

    public static ProductGroupText Create(ProductGroupId groupId, string language, DateTimeOffset now)
    {
        // The language comes from the validated request or the fixed language list.
        if (!Languages.Contains(language))
            throw new ArgumentOutOfRangeException(nameof(language), language, "Expected uk or ru.");

        return new ProductGroupText { GroupId = groupId, Language = language, UpdatedAt = now };
    }

    /// <summary>Replaces all four texts; blanks are stored as empty. Returns whether anything changed.</summary>
    public DomainResult<bool> Update(string? shortDescription, string? description, string? seoTitle,
        string? seoDescription, DateTimeOffset now)
    {
        var checks = new DomainChecks();
        var values = (
            checks.Take(Text(shortDescription, ShortDescriptionMaxLength, nameof(ShortDescription))),
            checks.Take(Text(description, DescriptionMaxLength, nameof(Description))),
            checks.Take(Text(seoTitle, SeoTitleMaxLength, nameof(SeoTitle))),
            checks.Take(Text(seoDescription, SeoDescriptionMaxLength, nameof(SeoDescription))));
        if (checks.Error is { } error)
            return error;
        if (values == (ShortDescription, Description, SeoTitle, SeoDescription))
            return false;

        (ShortDescription, Description, SeoTitle, SeoDescription) = values;
        UpdatedAt = now;
        return true;
    }

    private static DomainResult<string> Text(string? value, int maxLength, string field)
    {
        var trimmed = value?.Trim() ?? string.Empty;
        if (trimmed.Length > maxLength)
            return ProductGroupErrors.TooLong(field, maxLength, trimmed.Length);

        return trimmed;
    }
}
