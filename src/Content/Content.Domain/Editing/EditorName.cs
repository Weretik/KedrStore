namespace Content.Domain.Editing;

/// <summary>Who saved a change, as the admin shows it: the editor's email or user name.</summary>
public static class EditorName
{
    public const int MaxLength = 256;

    /// <summary>The trimmed name, cut to <see cref="MaxLength"/>; «unknown» when the session gave none.</summary>
    public static string Normalize(string? name)
    {
        var value = string.IsNullOrWhiteSpace(name) ? "unknown" : name.Trim();
        return value.Length <= MaxLength ? value : value[..MaxLength];
    }
}
