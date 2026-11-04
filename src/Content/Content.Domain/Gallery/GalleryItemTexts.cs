namespace Content.Domain.Gallery;

/// <summary>
/// Optional title and caption in both site languages. An empty value is stored as <c>null</c>;
/// the site then shows the card without it.
/// </summary>
public sealed record GalleryItemTexts(string? TitleUk, string? TitleRu, string? CaptionUk, string? CaptionRu)
{
    public const int TitleMaxLength = 150;
    public const int CaptionMaxLength = 500;

    public static GalleryItemTexts Empty { get; } = new(null, null, null, null);

    public static GalleryItemTexts Create(string? titleUk, string? titleRu, string? captionUk, string? captionRu) =>
        new(Normalize(titleUk, nameof(TitleUk), TitleMaxLength),
            Normalize(titleRu, nameof(TitleRu), TitleMaxLength),
            Normalize(captionUk, nameof(CaptionUk), CaptionMaxLength),
            Normalize(captionRu, nameof(CaptionRu), CaptionMaxLength));

    private static string? Normalize(string? value, string field, int maxLength)
    {
        var trimmed = value?.Trim();
        if (string.IsNullOrEmpty(trimmed))
            return null;
        if (trimmed.Length > maxLength)
            throw new DomainException(GalleryErrors.TooLong(field, maxLength, trimmed.Length));
        return trimmed;
    }
}
