namespace Catalog.Application.Querying;

/// <summary>Catalog content languages: Ukrainian by default, Russian on request.</summary>
public static class CatalogLanguage
{
    public const string Ukrainian = "uk";
    public const string Russian = "ru";

    public static string Normalize(string? language)
        => string.Equals(language, Russian, StringComparison.OrdinalIgnoreCase) ? Russian : Ukrainian;
}
