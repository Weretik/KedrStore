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
        if (!Languages.Contains(language))
            throw new DomainException(ProductGroupErrors.Required(nameof(Language)));

        return new ProductGroupText { GroupId = groupId, Language = language, UpdatedAt = now };
    }

    /// <summary>Replaces all four texts; blanks are stored as empty. Returns whether anything changed.</summary>
    public bool Update(string? shortDescription, string? description, string? seoTitle, string? seoDescription,
        DateTimeOffset now)
    {
        var values = (
            Text(shortDescription, ShortDescriptionMaxLength, nameof(ShortDescription)),
            Text(description, DescriptionMaxLength, nameof(Description)),
            Text(seoTitle, SeoTitleMaxLength, nameof(SeoTitle)),
            Text(seoDescription, SeoDescriptionMaxLength, nameof(SeoDescription)));
        if (values == (ShortDescription, Description, SeoTitle, SeoDescription))
            return false;

        (ShortDescription, Description, SeoTitle, SeoDescription) = values;
        UpdatedAt = now;
        return true;
    }

    private static string Text(string? value, int maxLength, string field)
    {
        var trimmed = value?.Trim() ?? string.Empty;
        if (trimmed.Length > maxLength)
            throw new DomainException(ProductGroupErrors.TooLong(field, maxLength, trimmed.Length));

        return trimmed;
    }
}
