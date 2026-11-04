namespace Content.Domain.Home;

/// <summary>Checks shared by the FAQ item and the brand: required trimmed text, non-negative position.</summary>
internal static class HomeGuard
{
    internal static string Text(string? value, string field, int maxLength)
    {
        var trimmed = value?.Trim();
        if (string.IsNullOrEmpty(trimmed))
            throw new DomainException(HomeErrors.Required(field));
        if (trimmed.Length > maxLength)
            throw new DomainException(HomeErrors.TooLong(field, maxLength, trimmed.Length));
        return trimmed;
    }

    internal static int Position(int position) =>
        position < 0 ? throw new DomainException(HomeErrors.PositionInvalid(position)) : position;
}
