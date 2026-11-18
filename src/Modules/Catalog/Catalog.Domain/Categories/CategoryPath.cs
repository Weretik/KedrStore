namespace Catalog.Domain.Categories;

/// <summary>A category's place in the tree as a PostgreSQL <c>ltree</c> path (<c>n1.n12.n40</c>).</summary>
public readonly partial record struct CategoryPath : IComparable<CategoryPath>, IComparable
{
    private static readonly Regex _ltreeRegex = MyRegex();

    public string Value { get; }

    private CategoryPath(string value) => Value = value;

    public static CategoryPath None => Create("n0");

    /// <summary>A path the code builds; an empty or malformed one is a programming error.</summary>
    public static CategoryPath Create(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
            throw new ArgumentException("Потрібне місце категорії в дереві.", nameof(value));
        if (!_ltreeRegex.IsMatch(value))
            throw new ArgumentException("Некоректне місце категорії в дереві.", nameof(value));
        return new CategoryPath(value);
    }

    public static CategoryPath Root(string node) => Create($"n{node}");
    public CategoryPath Append(string node) => Create($"{Value}.n{node}");

    // The same order the Vogen type had (culture-aware string comparison).
    public int CompareTo(CategoryPath other) => string.Compare(Value, other.Value, StringComparison.CurrentCulture);

    int IComparable.CompareTo(object? obj) => obj is CategoryPath other ? CompareTo(other)
        : obj is null ? 1 : throw new ArgumentException("Expected " + nameof(CategoryPath) + ".", nameof(obj));

    public override string ToString() => Value;

    [GeneratedRegex(@"^[A-Za-z0-9_]+(\.[A-Za-z0-9_]+)*$", RegexOptions.Compiled)]
    private static partial Regex MyRegex();
}
