namespace Presentation.Shared.Extensions;

public static class StringNormalizeOrNull
{
    public static string? NormalizeOrNull(this string? value) => string.IsNullOrWhiteSpace(value) ? null : value.Trim();
}
