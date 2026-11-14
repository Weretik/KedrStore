namespace Content.Domain.Home;

/// <summary>Checks shared by the FAQ item and the brand: required trimmed text, non-negative position.</summary>
internal static class HomeGuard
{
    internal static DomainResult<string> Text(string? value, string field, int maxLength)
    {
        var trimmed = value?.Trim();
        if (string.IsNullOrEmpty(trimmed))
            return HomeErrors.Required(field);
        if (trimmed.Length > maxLength)
            return HomeErrors.TooLong(field, maxLength, trimmed.Length);
        return trimmed;
    }

    internal static DomainResult<int> Position(int position) =>
        position < 0 ? HomeErrors.PositionInvalid(position) : position;
}
