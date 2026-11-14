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

    public static DomainResult<GalleryItemTexts> Create(string? titleUk, string? titleRu, string? captionUk,
        string? captionRu)
    {
        var tUk = Normalize(titleUk, nameof(TitleUk), TitleMaxLength);
        var tRu = Normalize(titleRu, nameof(TitleRu), TitleMaxLength);
        var cUk = Normalize(captionUk, nameof(CaptionUk), CaptionMaxLength);
        var cRu = Normalize(captionRu, nameof(CaptionRu), CaptionMaxLength);
        var valid = DomainResult.Combine(tUk, tRu, cUk, cRu);
        return valid.IsSuccess
            ? new GalleryItemTexts(tUk.Value, tRu.Value, cUk.Value, cRu.Value)
            : valid.Error!;
    }

    private static DomainResult<string?> Normalize(string? value, string field, int maxLength)
    {
        var trimmed = value?.Trim();
        if (string.IsNullOrEmpty(trimmed))
            return DomainResult<string?>.Success(null);
        if (trimmed.Length > maxLength)
            return GalleryErrors.TooLong(field, maxLength, trimmed.Length);
        return trimmed;
    }
}
