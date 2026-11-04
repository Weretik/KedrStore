namespace Content.Domain.Articles;

/// <summary>Database identity of an article; the public address uses <see cref="Article.Slug"/>.</summary>
public readonly record struct ArticleId : IComparable<ArticleId>
{
    public long Value { get; }

    private ArticleId(long value) => Value = value;

    public static ArticleId Create(long value) => value > 0
        ? new ArticleId(value)
        : throw new ArgumentOutOfRangeException(nameof(value));

    public static ArticleId FromStorage(long value) => new(value);

    public int CompareTo(ArticleId other) => Value.CompareTo(other.Value);
}
