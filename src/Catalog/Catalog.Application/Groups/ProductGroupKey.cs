namespace Catalog.Application.Groups;

/// <summary>The automatic group key: the base name with single spaces, compared without case.</summary>
public static class ProductGroupKey
{
    public static string DisplayName(string baseName)
        => string.Join(' ', baseName.Split((char[]?)null, StringSplitOptions.RemoveEmptyEntries));

    public static string From(string baseName) => DisplayName(baseName).ToLowerInvariant();
}
