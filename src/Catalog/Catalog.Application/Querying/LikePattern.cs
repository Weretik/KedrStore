namespace Catalog.Application.Querying;

/// <summary>Builds PostgreSQL ILIKE patterns with <c>\</c> as the escape character.</summary>
public static class LikePattern
{
    public const string EscapeCharacter = @"\";

    /// <summary>Escapes <c>\</c>, <c>%</c> and <c>_</c> so the text matches literally.</summary>
    public static string Escape(string text) => text
        .Replace(@"\", @"\\", StringComparison.Ordinal)
        .Replace("%", @"\%", StringComparison.Ordinal)
        .Replace("_", @"\_", StringComparison.Ordinal);
}
