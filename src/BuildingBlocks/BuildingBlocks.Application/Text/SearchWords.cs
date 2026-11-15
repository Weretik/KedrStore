namespace BuildingBlocks.Application.Text;

/// <summary>
/// Search input as <c>ILIKE</c> patterns (database rules 2.1): the text is split into words, each word is escaped and
/// becomes <c>%word%</c>. A query adds one condition per pattern, so every word must match and a part of a word is
/// enough; a <c>gin_trgm_ops</c> index on the column serves each condition.
/// </summary>
public static class SearchWords
{
    /// <summary>The escape character to pass to <c>EF.Functions.ILike</c> with these patterns.</summary>
    public const string EscapeCharacter = @"\";

    /// <summary>One <c>%word%</c> pattern per word of <paramref name="text"/>; empty for blank text.</summary>
    public static IReadOnlyList<string> Patterns(string? text) =>
        string.IsNullOrWhiteSpace(text)
            ? []
            : text.Split((char[]?)null, StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
                .Select(word => "%" + Escape(word) + "%")
                .ToArray();

    /// <summary>Escapes <c>\</c>, <c>%</c> and <c>_</c> so the text matches literally.</summary>
    public static string Escape(string text) => text
        .Replace(@"\", @"\\", StringComparison.Ordinal)
        .Replace("%", @"\%", StringComparison.Ordinal)
        .Replace("_", @"\_", StringComparison.Ordinal);
}
