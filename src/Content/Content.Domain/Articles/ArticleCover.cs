namespace Content.Domain.Articles;

/// <summary>Cover image bytes of one article, kept apart so lists never load them.</summary>
public sealed class ArticleCover : IAggregateRoot
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
        if (data.Length is 0 or > MaxBytes)
            throw new DomainException(ArticleErrors.TooLong("Cover", MaxBytes, data.Length));

        Data = data;
        ContentType = contentType;
    }
}
