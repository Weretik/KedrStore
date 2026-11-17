namespace Content.Domain.Articles;

/// <summary>
/// Legacy cover bytes of one article, a child of <see cref="Article"/> kept in its own table so lists never load them.
/// Only the article removes it, when a library picture replaces the legacy cover or the cover is removed.
/// </summary>
public sealed class ArticleCover
{
    public const int MaxBytes = 1_000_000;

    public ArticleId ArticleId { get; private set; }
    public byte[] Data { get; private set; } = [];
    public string ContentType { get; private set; } = null!;

    private ArticleCover() { }

    public static ArticleCover Create(ArticleId articleId, byte[] data, string contentType)
    {
        var cover = new ArticleCover { ArticleId = articleId };
        cover.Replace(data, contentType);
        return cover;
    }

    public void Replace(byte[] data, string contentType)
    {
        ArgumentNullException.ThrowIfNull(data);
        ArgumentException.ThrowIfNullOrWhiteSpace(contentType);
        // Legacy covers are no longer uploaded; only stored rows and test data reach this.
        if (data.Length is 0 or > MaxBytes)
            throw new ArgumentOutOfRangeException(nameof(data), data.Length, $"A cover holds 1-{MaxBytes} bytes.");

        Data = data;
        ContentType = contentType;
    }
}
